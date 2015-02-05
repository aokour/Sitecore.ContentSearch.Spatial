using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Search.Function;
using Lucene.Net.Spatial.Util;
using Lucene.Net.Spatial.Vector;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Shapes;
using Spatial4n.Core.Shapes.Impl;
using System.Diagnostics;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Util;
using System.Collections.Concurrent;
using Sitecore.ContentSearch.Spatial.Common;

namespace Sitecore.ContentSearch.Spatial.Query
{
    public class DistanceReverseValueSource : DistanceValueSource
    {
        private readonly PointVectorStrategy strategy;
        private readonly Point from;
        private readonly double max;

        public DistanceReverseValueSource(PointVectorStrategy strategy, Point from, double max)
            : base(strategy, from)
        {
            this.strategy = strategy;
            this.from = from;
            this.max = max;
        }

        public class DistanceReverseDocValues : DocValues
        {
            private readonly DistanceValueSource enclosingInstance;

            private readonly double[] ptX, ptY;
            private readonly IBits validX, validY;

            private readonly Point from;
            private readonly DistanceCalculator calculator;
            private readonly double nullValue;
            private readonly double max;

            public DistanceReverseDocValues(DistanceReverseValueSource enclosingInstance, IndexReader reader, double max)
            {
                this.enclosingInstance = enclosingInstance;

                ptX = FieldCache_Fields.DEFAULT.GetDoubles(reader, enclosingInstance.strategy.GetFieldNameX()/*, true*/);
                ptY = FieldCache_Fields.DEFAULT.GetDoubles(reader, enclosingInstance.strategy.GetFieldNameY()/*, true*/);
                validX = FieldCache_Fields.DEFAULT.GetDocsWithField(reader, enclosingInstance.strategy.GetFieldNameX());
                validY = FieldCache_Fields.DEFAULT.GetDocsWithField(reader, enclosingInstance.strategy.GetFieldNameY());

                from = enclosingInstance.from;
                calculator = enclosingInstance.strategy.GetSpatialContext().GetDistCalc();
                nullValue = (enclosingInstance.strategy.GetSpatialContext().IsGeo() ? 180 : double.MaxValue);
                this.max = max;
            }

            public override float FloatVal(int doc)
            {
                return ((float)max - (float)DoubleVal(doc));

            }

            public override double DoubleVal(int doc)
            {
                // make sure it has minX and area
                try
                {
                    if (validX.Get(doc))
                    {
                        Debug.Assert(validY.Get(doc));
                        return calculator.Distance(from, ptX[doc], ptY[doc]);
                    }
                }
                catch(Exception ex)
                {

                }
                return nullValue;
            }

            public override string ToString(int doc)
            {
                return enclosingInstance.Description() + "=" + DoubleVal(doc);
            }
        }

        public override DocValues GetValues(IndexReader reader)
        {
            return new DistanceReverseDocValues(this, reader, max);
        }
    }

}
