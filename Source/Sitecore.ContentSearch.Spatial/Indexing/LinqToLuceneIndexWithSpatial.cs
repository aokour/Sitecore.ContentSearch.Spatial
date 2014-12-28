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

namespace Sitecore.ContentSearch.Spatial.Indexing
{
    public class LinqToLuceneIndexWithSpatial<TItem> : LinqToLuceneIndex<TItem>
    {
        private IExecutionContext[] executionContext { get; set; }
        private LuceneSearchContext context { get; set; }

        private QueryMapper<LuceneQuery> queryMapper;
        private LuceneQueryOptimizer queryOptimizer;

        private LinqToLuceneIndex<TItem> linqToLuceneIndex;

        public LinqToLuceneIndexWithSpatial(LuceneSearchWithSpatialContext context)
            : this(context, (IExecutionContext[])null)
        {

            }

        public LinqToLuceneIndexWithSpatial(LuceneSearchWithSpatialContext context, IExecutionContext executionContext)
            : this (context,new IExecutionContext[] {executionContext}) {
        }

        public LinqToLuceneIndexWithSpatial(LuceneSearchWithSpatialContext context, IExecutionContext[] executionContext)
            : base(context, executionContext)
        {
            this.context = context;
            this.executionContext = executionContext;

            queryMapper = new Sitecore.ContentSearch.Spatial.Query.LuceneQueryMapperWithSpatial(new LuceneIndexParameters(context.Index.Configuration.IndexFieldStorageValueFormatter, ((LuceneIndexConfiguration)context.Index.Configuration).Analyzer, context.Index.Configuration.VirtualFieldProcessors, context.Index.FieldNameTranslator, executionContext));
            queryOptimizer = new Sitecore.ContentSearch.Spatial.Query.LuceneQueryOptimizerWithSpatial();
            linqToLuceneIndex = new LinqToLuceneIndex<TItem>(context, executionContext);
        }

        public override IQueryable<TItem> GetQueryable()
        {
            Sitecore.ContentSearch.Spatial.Parsing.GenericQueryableWithSpatial<TItem, LuceneQuery> genericQueryable = new Sitecore.ContentSearch.Spatial.Parsing.GenericQueryableWithSpatial<TItem, LuceneQuery>(linqToLuceneIndex, QueryMapper, QueryOptimizer, FieldNameTranslator);

            return genericQueryable;
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