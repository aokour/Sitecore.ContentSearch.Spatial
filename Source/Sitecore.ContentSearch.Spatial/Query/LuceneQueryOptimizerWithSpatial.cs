using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch.Spatial.Linq.Nodes;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Queries;
using Sitecore.ContentSearch.Linq.Lucene;
using Sitecore.ContentSearch.Linq.Nodes;
using Spatial4n.Core.Context;
using Spatial4n.Core.Shapes;

namespace Sitecore.ContentSearch.Spatial.Query
{
    public class LuceneQueryOptimizerWithSpatial : LuceneQueryOptimizer
    {
        protected override Sitecore.ContentSearch.Linq.Nodes.QueryNode Visit(Sitecore.ContentSearch.Linq.Nodes.QueryNode node, LuceneQueryOptimizerState state)
        {
            var withinRadiusNode = node as WithinRadiusNode;
            if (withinRadiusNode != null)
            {
                return VisitWithinRadius(withinRadiusNode, state);
            }
            else
                return base.Visit(node, state);
        }

        protected virtual QueryNode VisitWithinRadius(WithinRadiusNode node, LuceneQueryOptimizerState state)
        {
            if(node.SourceNode==null)
                return new WithinRadiusNode(null, node.Field, node.Latitude, node.Longitude, node.Radius, node.SortByDistance);
            else
                return new WithinRadiusNode(this.Visit(node.SourceNode, state), node.Field, node.Latitude, node.Longitude, node.Radius, node.SortByDistance);
        }
    }
}