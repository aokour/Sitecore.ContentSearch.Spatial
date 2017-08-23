using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Search.Function;
using Lucene.Net.Spatial;
using Lucene.Net.Spatial.Util;
using Lucene.Net.Spatial.Vector;
using Sitecore.ContentSearch.Linq.Common;
using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Shapes;
using System;

namespace Sitecore.ContentSearch.Spatial
{
    public class DistanceValueSource : Lucene.Net.Spatial.Vector.DistanceValueSource
    {
        private readonly PointVectorStrategy strategy;

        private readonly Point @from;

        private readonly SortDirection direction;

        public DistanceValueSource(PointVectorStrategy strategy, Point from, SortDirection direction) : base(strategy, from)
        {
            this.strategy = strategy;
            this.@from = from;
            this.direction = direction;
        }

        public override DocValues GetValues(IndexReader reader)
        {
            return new DistanceValueSource.DistanceDocValues(this, reader);
        }

        internal new class DistanceDocValues : DocValues
        {
            private readonly DistanceValueSource enclosingInstance;

            private readonly double[] ptX;

            private readonly double[] ptY;

            private readonly IBits validX;

            private readonly IBits validY;

            private readonly Point @from;

            private readonly DistanceCalculator calculator;

            private readonly double nullValue;

            public DistanceDocValues(DistanceValueSource enclosingInstance, IndexReader reader)
            {
                this.enclosingInstance = enclosingInstance;
                this.ptX = FieldCache_Fields.DEFAULT.GetDoubles(reader, enclosingInstance.strategy.GetFieldNameX());
                this.ptY = FieldCache_Fields.DEFAULT.GetDoubles(reader, enclosingInstance.strategy.GetFieldNameY());
                this.validX = FieldCache_Fields.DEFAULT.GetDocsWithField(reader, enclosingInstance.strategy.GetFieldNameX());
                this.validY = FieldCache_Fields.DEFAULT.GetDocsWithField(reader, enclosingInstance.strategy.GetFieldNameY());
                this.@from = enclosingInstance.@from;
                this.calculator = enclosingInstance.strategy.GetSpatialContext().GetDistCalc();
                this.nullValue = (enclosingInstance.strategy.GetSpatialContext().IsGeo() ? 180 : double.MaxValue);
            }

            public override double DoubleVal(int doc)
            {
                if (!this.validX.Get(doc) || !this.validY.Get(doc))
                {
                    return this.nullValue;
                }
                return this.calculator.Distance(this.@from, this.ptX[doc], this.ptY[doc]);
            }

            public override float FloatVal(int doc)
            {
                if (this.enclosingInstance.direction != null)
                {
                    return (float)this.DoubleVal(doc);
                }
                return (float)(180 - this.DoubleVal(doc));
            }

            public override string ToString(int doc)
            {
                return string.Concat(this.enclosingInstance.Description(), "=", this.DoubleVal(doc));
            }
        }
    }
}