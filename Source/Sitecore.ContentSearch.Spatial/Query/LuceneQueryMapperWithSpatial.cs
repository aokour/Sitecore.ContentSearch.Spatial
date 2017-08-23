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
                //if (node is OrderByDistanceNode)
                //{
                //    return this.VisitOrderByDistance((OrderByDistanceNode)node, state);
                //}
            
            return base.Visit(node, state);
        }
        
        protected virtual Lucene.Net.Search.Query VisitWithinRadius(WithinRadiusNode node, LuceneQueryMapperState mappingState)
        {
            //SpatialContext ctx = SpatialContext.GEO;
           
            //var strategy = new PointVectorStrategy(ctx, Sitecore.ContentSearch.Spatial.Common.Constants.LocationFieldName);
            
            //    var distance = DistanceUtils.Dist2Degrees((double)node.Radius, DistanceUtils.EARTH_MEAN_RADIUS_MI);
            //    Circle circle = ctx.MakeCircle((double)node.Longitude,(double)node.Latitude, distance);

            //    var spatialArgs = new SpatialArgs(SpatialOperation.IsWithin, circle);
            //    var dq = strategy.MakeQuery(spatialArgs);

            //    //DistanceReverseValueSource valueSource = new DistanceReverseValueSource(strategy, circle.GetCenter(), distance);
            //    DistanceValueSource valueSource = new DistanceValueSource(strategy, circle.GetCenter());
            //    ValueSourceFilter vsf = new ValueSourceFilter(new QueryWrapperFilter(dq), valueSource, 0, distance);
            //    var filteredSpatial = new FilteredQuery(new MatchAllDocsQuery(), vsf);
            //    mappingState.FilterQuery = filteredSpatial;
            //    Lucene.Net.Search.Query spatialRankingQuery = new FunctionQuery(valueSource);
            //    Random r = new Random(DateTime.Now.Millisecond);
            //    var randomNumber = r.Next(10000101,11000101);
            //    Lucene.Net.Search.Query dummyQuery = Lucene.Net.Search.NumericRangeQuery.NewIntRange("__smallcreateddate", randomNumber, Int32.Parse(DateTime.Now.ToString("yyyyMMdd")), true, true);
            //    BooleanQuery bq = new BooleanQuery();

            //    bq.Add(filteredSpatial, Occur.MUST);
            //    bq.Add(spatialRankingQuery, Occur.MUST);
            //    bq.Add(dummyQuery, Occur.SHOULD);
            //    return bq;
            
           

            Point point = SpatialContext.GEO.MakePoint((double)node.Longitude, (double)node.Latitude);
            PointVectorStrategy pointVectorStrategy = new PointVectorStrategy(SpatialContext.GEO, node.Field);
            ValueSource valueSource = pointVectorStrategy.MakeDistanceValueSource(point);
            double num = DistanceUtils.Dist2Degrees((double)node.Radius, DistanceUtils.EARTH_MEAN_RADIUS_MI );
            Circle circle = SpatialContext.GEO.MakeCircle(point, num);
            SpatialArgs spatialArg = new SpatialArgs(SpatialOperation.IsWithin, circle);
            ValueSourceFilter valueSourceFilter = new ValueSourceFilter(new QueryWrapperFilter(pointVectorStrategy.MakeQuery(spatialArg)), valueSource, 0, num);
            FilteredQuery filteredQuery = new FilteredQuery(new MatchAllDocsQuery(), valueSourceFilter);
            Random r = new Random(DateTime.Now.Millisecond);
            //var randomNumber = r.Next(10000101,11000101);
            //Lucene.Net.Search.Query dummyQuery = Lucene.Net.Search.NumericRangeQuery.NewIntRange("__smallcreateddate", randomNumber, Int32.Parse(DateTime.Now.ToString("yyyyMMdd")), true, true);
            if (node.SourceNode != null)
                return this.CombineQueries(filteredQuery, this.Visit(node.SourceNode, mappingState));
            else
            {
                BooleanQuery bq = new BooleanQuery();

                //bq.Add(filteredQuery, Occur.MUST);
                //bq.Add(dummyQuery, Occur.SHOULD);
                //return bq;
                return filteredQuery;
            }
            

            //using (var analyser = new StandardAnalyzer(new Lucene.Net.Util.Version()))
            //{
            //    var distance = DistanceUtils.Dist2Degrees((double)node.Radius, DistanceUtils.EARTH_MEAN_RADIUS_MI);
            //    var searchArea = SpatialContext.GEO.MakeCircle((double)node.Longitude, (double)node.Latitude, distance);

            //    var fields = new[] { node.Field };
            //    var parser = new MultiFieldQueryParser(LuceneVersion, fields, analyser);
            //    parser.DefaultOperator = QueryParser.Operator.OR; // Allow multiple terms.
            //    //var query = ParseQuery(queryString, parser);

            //    var spatialArgs = new SpatialArgs(SpatialOperation.Intersects, searchArea);
            //    var spatialQuery = _strategy.MakeQuery(spatialArgs);
            //    var valueSource = _strategy.MakeRecipDistanceValueSource(searchArea);
            //    var valueSourceFilter = new ValueSourceFilter(new QueryWrapperFilter(spatialQuery), valueSource, 0, 1);

            //    var filteredSpatial = new FilteredQuery(query, valueSourceFilter);
            //    var spatialRankingQuery = new FunctionQuery(valueSource);

            //    var bq = new BooleanQuery();
            //    bq.Add(filteredSpatial, Occur.MUST);
            //    bq.Add(spatialRankingQuery, Occur.MUST);

            //    var hits = searcher.Search(bq, maxHits).ScoreDocs;

            //    results = MapResultsToSearchItems(hits, searcher);
            //}
        }

       
    }
}