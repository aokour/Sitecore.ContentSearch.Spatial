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


        protected virtual Sitecore.ContentSearch.Linq.Nodes.QueryNode VisitWithinRadius(WithinRadiusNode node, LuceneQueryOptimizerState mappingState)
        {
            return new WithinRadiusNode(node.Field, node.Latitude, node.Longitude, node.Radius, mappingState.Boost);
        }
    }
}