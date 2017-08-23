using Lucene.Net.Search.Function;
using Lucene.Net.Spatial.Vector;
using Spatial4n.Core.Context;
using Spatial4n.Core.Shapes;
using System;

namespace Sitecore.ContentSearch.Spatial
{
    public class PointVectorStrategy : Lucene.Net.Spatial.Vector.PointVectorStrategy
    {
        public PointVectorStrategy(SpatialContext ctx, string fieldNamePrefix) : base(ctx, fieldNamePrefix)
        {
        }

        public override ValueSource MakeDistanceValueSource(Point queryPoint)
        {
            return new DistanceValueSource(this, queryPoint, 0);
        }
    }
}