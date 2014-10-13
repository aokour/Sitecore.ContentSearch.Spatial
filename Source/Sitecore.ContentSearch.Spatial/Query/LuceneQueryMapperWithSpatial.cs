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

namespace Sitecore.ContentSearch.Spatial.Query
{
    public class LuceneQueryMapperWithSpatial :LuceneQueryMapper
    {
        public LuceneQueryMapperWithSpatial(LuceneIndexParameters parameters)
            : base(parameters)
        {
        }

       SpatialContext ctx =  SpatialContext.GEO;
        protected override Lucene.Net.Search.Query Visit(Sitecore.ContentSearch.Linq.Nodes.QueryNode node, LuceneQueryMapperState mappingState)
        {
            var withinRadiusNode = node as WithinRadiusNode;
            if (withinRadiusNode != null)
            {
                return VisitWithinRadius(withinRadiusNode, mappingState);
            }
            else
                return base.Visit(node, mappingState);
        }

        protected virtual Lucene.Net.Search.Query VisitWithinRadius(WithinRadiusNode node, LuceneQueryMapperState mappingState)
        {
            SpatialContext ctx = SpatialContext.GEO;
           
            var strategy = new PointVectorStrategy(ctx, Sitecore.ContentSearch.Spatial.Common.Constants.LocationFieldName);

            if (node.Latitude is double && node.Longitude is double && node.Radius is double)
            {
                var distance = DistanceUtils.Dist2Degrees((double)node.Radius, DistanceUtils.EARTH_MEAN_RADIUS_MI);
                Circle circle = ctx.MakeCircle((double)node.Longitude,(double)node.Latitude, distance);

                var spatialArgs = new SpatialArgs(SpatialOperation.Intersects, circle);
                var dq = strategy.MakeQuery(spatialArgs);

                ValueSource valueSource = strategy.MakeDistanceValueSource(circle.GetCenter());
                FunctionQuery sortQuery = new FunctionQuery(valueSource);
            
                ValueSourceFilter vsf = new ValueSourceFilter(new QueryWrapperFilter(dq), valueSource, 0, distance);
                var filteredSpatial = new FilteredQuery(new MatchAllDocsQuery(), vsf);
                var spatialRankingQuery = new FunctionQuery(valueSource);
                BooleanQuery bq = new BooleanQuery();
                bq.Add(filteredSpatial, Occur.MUST);
                bq.Add(spatialRankingQuery, Occur.MUST);

                return bq;
            }
            throw new NotSupportedException("Wrong parameters type, Radius, latitude and longitude must be of type double");
        }
    }
}