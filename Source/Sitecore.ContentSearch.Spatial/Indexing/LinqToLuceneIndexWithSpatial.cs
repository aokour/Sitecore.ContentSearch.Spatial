using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Linq.Lucene;
using Sitecore.ContentSearch.Linq.Methods;
using Sitecore.ContentSearch.Linq.Nodes;
using Sitecore.ContentSearch.Linq.Parsing;
using Sitecore.ContentSearch.LuceneProvider;
using Sitecore.ContentSearch.Pipelines.GetFacets;
using Sitecore.ContentSearch.Spatial.Provider.Lucene;
using Sitecore.ContentSearch.Spatial.Parsing;

namespace Sitecore.ContentSearch.Spatial.Indexing
{
    public class LinqToLuceneIndexWithSpatial<TItem> : LinqToLuceneIndex<TItem>
    {
        private IExecutionContext[] executionContext { get; set; }
        private LuceneSearchContext context { get; set; }

        private QueryMapper<LuceneQuery> queryMapper;
        private LuceneQueryOptimizer queryOptimizer;

        private LinqToLuceneIndex<TItem> linqToLuceneIndex;

        public LinqToLuceneIndexWithSpatial(LuceneSearchContext context) : this(context, new IExecutionContext[1])
        {
        }

        public LinqToLuceneIndexWithSpatial(LuceneSearchContext context, IExecutionContext executionContext) : this(context, new IExecutionContext[] { executionContext })
        {
        }

        public LinqToLuceneIndexWithSpatial(LuceneSearchContext context, IExecutionContext[] executionContext)
            : base(context, executionContext)
        {
            this.context = context;
            this.executionContext = executionContext;
            

            LuceneIndexConfiguration configuration = (LuceneIndexConfiguration)context.Index.Configuration;
            LuceneIndexParameters luceneIndexParameter = new LuceneIndexParameters(configuration.IndexFieldStorageValueFormatter, configuration.Analyzer, configuration.VirtualFields, context.Index.FieldNameTranslator, executionContext);
            queryMapper = new Sitecore.ContentSearch.Spatial.Query.LuceneQueryMapperWithSpatial(luceneIndexParameter);
            queryOptimizer = new Sitecore.ContentSearch.Spatial.Query.LuceneQueryOptimizerWithSpatial();
        }

        public override IQueryable<TItem> GetQueryable()
        {
            ExpressionParserWithSpatial expressionParser = new ExpressionParserWithSpatial(typeof(TItem), this.ItemType, this.FieldNameTranslator);
            IQueryable<TItem> genericQueryable = new GenericQueryableWithSpatial<TItem, LuceneQuery>(this, this.QueryMapper, this.QueryOptimizer, this.FieldNameTranslator, expressionParser);
      
            (genericQueryable as IHasTraceWriter).TraceWriter = ((IHasTraceWriter)this).TraceWriter;
            return this.GetTypeInheritance(typeof(TItem)).SelectMany<Type, IPredefinedQueryAttribute>((Type t) => t.GetCustomAttributes(typeof(IPredefinedQueryAttribute), true).Cast<IPredefinedQueryAttribute>()).Aggregate<IPredefinedQueryAttribute, IQueryable<TItem>>(genericQueryable, (IQueryable<TItem> q, IPredefinedQueryAttribute a) => a.ApplyFilter<TItem>(q, this.ValueFormatter));
            
        }

        protected object ApplyScalarMethods<TResult, TDocument>(object query, object processedResults, object results)
        {
            return (base.GetType().BaseType ?? base.GetType()).GetMethod("ApplyScalarMethods", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(new Type[] { typeof(TResult), typeof(TDocument) }).Invoke(this, new object[] { query, processedResults, results });
        }

        protected object ApplySearchMethods<TElement>(object query, object searchHits)
        {
            return (base.GetType().BaseType ?? base.GetType()).GetMethod("ApplySearchMethods", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(new Type[] { typeof(TElement) }).Invoke(this, new object[] { query, searchHits });
        }

        private IEnumerable<Type> GetTypeInheritance(Type type)
        {
            Type i;
            yield return type;
            for (i = type.BaseType; i != null; i = i.BaseType)
            {
                yield return i;
            }
            i = null;
        }

        protected override QueryMapper<LuceneQuery> QueryMapper
        {
            get
            {
                return queryMapper;
            }
        }

        

        protected override IQueryOptimizer QueryOptimizer
        {
            get
            {
                return queryOptimizer;
            }
        }
    }
}