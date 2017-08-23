using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Indexing;
using Sitecore.ContentSearch.Linq.Parsing;
using System.IO;

namespace Sitecore.ContentSearch.Spatial.Parsing
{
    public class GenericQueryableWithSpatial<TElement, TQuery> : GenericQueryable<TElement, TQuery>
    {
        private readonly ExpressionParser _expressionParser;

        public GenericQueryableWithSpatial(Index<TElement, TQuery> index, QueryMapper<TQuery> queryMapper, IQueryOptimizer queryOptimizer, FieldNameTranslator fieldNameTranslator, ExpressionParser expressionParser) : base(index, queryMapper, queryOptimizer, fieldNameTranslator)
        {
            this._expressionParser = expressionParser;
        }

        protected GenericQueryableWithSpatial(Index<TQuery> index, QueryMapper<TQuery> queryMapper, IQueryOptimizer queryOptimizer, Expression expression, Type itemType, FieldNameTranslator fieldNameTranslator, ExpressionParser expressionParser) : base(index, queryMapper, queryOptimizer, expression, itemType, fieldNameTranslator)
        {
            this._expressionParser = expressionParser;
        }

        public override IQueryable<TQueryElement> CreateQuery<TQueryElement>(Expression expression)
        {
            return new GenericQueryableWithSpatial<TQueryElement, TQuery>(base.Index, base.QueryMapper, base.QueryOptimizer, expression, base.ItemType, base.FieldNameTranslator, this._expressionParser);
        }


        protected GenericQueryableWithSpatial(Index<TQuery> index, QueryMapper<TQuery> queryMapper, IQueryOptimizer queryOptimizer, Expression expression, Type itemType, FieldNameTranslator fieldNameTranslator)
			: base(index, queryMapper, queryOptimizer, expression, itemType, fieldNameTranslator) { }

        protected override TQuery GetQuery(Expression expression)
        {
            this.Trace(expression, "Expression");
            IndexQuery indexQuery = this._expressionParser.Parse(expression);
            this.Trace(indexQuery, "Raw query:");
            IndexQuery indexQuery1 = base.QueryOptimizer.Optimize(indexQuery);
            this.Trace(indexQuery1, "Optimized query:");
            TQuery tQuery = base.QueryMapper.MapQuery(indexQuery1);
            this.Trace(new GenericDumpable((object)tQuery), "Native query:");
            return tQuery;

            //Trace(expression, "Expression");

            //// here we inject a special expression parser that fixes a few issues slated to be resolved in later releases of SC7
            //IndexQuery rawQuery = new ExpressionParserWithSpatial(typeof(TElement), ItemType, FieldNameTranslator).Parse(expression);

            //Trace(rawQuery, "Raw query:");
            //IndexQuery optimizedQuery = QueryOptimizer.Optimize(rawQuery);
            //Trace(optimizedQuery, "Optimized query:");
            //TQuery mappedQuery = QueryMapper.MapQuery(optimizedQuery);

            //return mappedQuery;

            //Trace(expression, "Expression");
            //IndexQuery indexQuery = (new ExpressionParserWithSpatial(typeof(TElement), this.ItemType, this.FieldNameTranslator)).Parse(expression);
            //this.Trace(indexQuery, "Raw query:");
            //IndexQuery indexQuery1 = this.QueryOptimizer.Optimize(indexQuery);
            //this.Trace(indexQuery1, "Optimized query:");
            //TQuery tQuery = this.QueryMapper.MapQuery(indexQuery1);
            //this.Trace(new GenericDumpable((object)tQuery), "Native query:");
            //return tQuery;
        }
        
    }

    internal class GenericDumpable : IDumpable
    {
        protected object Value
        {
            get;
            set;
        }

        public GenericDumpable(object value)
        {
            this.Value = value;
        }

        public virtual void WriteTo(TextWriter writer)
        {
            IDumpable value = this.Value as IDumpable;
            if (value != null)
            {
                value.WriteTo(writer);
                return;
            }
            writer.WriteLine(this.Value);
        }
    }
}