using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch.Spatial.Linq.Nodes;
using Lucene.Net.Search;
using Lucene.Net.Search.Function;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Queries;
using Lucene.Net.Spatial.Util;
using Lucene.Net.Spatial.Vector;
using Sitecore.ContentSearch.Linq.Lucene;
using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Shapes;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;

namespace Sitecore.ContentSearch.Spatial.Query
{
    public class LuceneQueryMapperWithSpatial :LuceneQueryMapper
    {
        public LuceneQueryMapperWithSpatial(LuceneIndexParameters parameters)
            : base(parameters)
        {
        }

        private BooleanQuery CombineQueries( Lucene.Net.Search.Query predicateQuery, Lucene.Net.Search.Query sourceQuery)
        {
            BooleanQuery booleanQueries = new BooleanQuery();
            Lucene.Net.Search.Query[] queryArray = new Lucene.Net.Search.Query[] { predicateQuery, sourceQuery };
            for (int i = 0; i < (int)queryArray.Length; i++)
            {
                Lucene.Net.Search.Query query = queryArray[i];
                if (!(query is MatchAllDocsQuery))
                {
                    booleanQueries.Add(query, Occur.MUST);
                }
            }
            return booleanQueries;
        }

        protected override Lucene.Net.Search.Query Visit(Sitecore.ContentSearch.Linq.Nodes.QueryNode node, LuceneQueryMapper.LuceneQueryMapperState state)
        {
                if (node is WithinRadiusNode)
                {
                    return this.VisitWithinRadius((WithinRadiusNode)node, state);
                }
            
            return base.Visit(node, state);
        }
        
        protected virtual Lucene.Net.Search.Query VisitWithinRadius(WithinRadiusNode node, LuceneQueryMapperState mappingState)
        {
            bool sortByDistance = (bool)node.SortByDistance;
            Point point = SpatialContext.GEO.MakePoint((double)node.Longitude, (double)node.Latitude);
            PointVectorStrategy pointVectorStrategy = new PointVectorStrategy(SpatialContext.GEO, node.Field);
            ValueSource valueSource = (!sortByDistance ? pointVectorStrategy.MakeDistanceValueSource(point)
                                       : new DistanceValueSource(pointVectorStrategy,point, ContentSearch.Linq.Common.SortDirection.Descending)) ;
            double num = DistanceUtils.Dist2Degrees((double)node.Radius, DistanceUtils.EARTH_MEAN_RADIUS_MI );
            Circle circle = SpatialContext.GEO.MakeCircle(point, num);
            SpatialArgs spatialArg = new SpatialArgs(SpatialOperation.IsWithin, circle);
            ValueSourceFilter valueSourceFilter = new ValueSourceFilter(new QueryWrapperFilter(pointVectorStrategy.MakeQuery(spatialArg)), valueSource, 0, num);
            FilteredQuery filteredQuery = new FilteredQuery(new MatchAllDocsQuery(), valueSourceFilter);
            Random r = new Random(DateTime.Now.Millisecond);
           if (node.SourceNode != null)
                return this.CombineQueries(filteredQuery, this.Visit(node.SourceNode, mappingState));
            else
            {
                BooleanQuery bq = new BooleanQuery();
                
                return filteredQuery;
            }
        }

       
    }
}