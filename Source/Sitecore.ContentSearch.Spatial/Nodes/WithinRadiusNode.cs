using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch.Linq.Nodes;

namespace Sitecore.ContentSearch.Spatial.Linq.Nodes
{
    public class WithinRadiusNode: QueryNode
    {

        public float Boost { get; protected set; }
        public string Field { get; protected set; }
        public object Latitude { get; protected set; }
        public object Longitude { get; protected set; }
        public object Radius { get; protected set; }

        public QueryNode SourceNode
        {
            get;
            protected set;
        }


        public override QueryNodeType NodeType
        {
            get { return QueryNodeType.LessThanOrEqual; }
        }

        public override IEnumerable<QueryNode> SubNodes
        {
            get {
                return new List<QueryNode>();
            
            }
        }

        public WithinRadiusNode(QueryNode sourceNode,string field, object lat, object lng, object withinRadiusinMiles)
            : this(sourceNode, field, lat, lng, withinRadiusinMiles, 1f)
        {

        }

        public WithinRadiusNode(QueryNode sourceNode, string field, object lat, object lng, object withinRadiusinMiles, float boost)
        {
            sourceNode = SourceNode;
            Latitude = lat;
            Longitude = lng;
            Radius = withinRadiusinMiles;
            this.Boost = boost;
            Field = field;
        }


        
    }
}