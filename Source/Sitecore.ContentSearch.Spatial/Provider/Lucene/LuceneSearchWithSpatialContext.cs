using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch.Spatial.Indexing;
using Lucene.Net.Analysis;
using Lucene.Net.Search;
using Sitecore.ContentSearch.Abstractions;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.LuceneProvider;
using Sitecore.ContentSearch.Pipelines.QueryGlobalFilters;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Diagnostics;
using Sitecore.Abstractions;
using Sitecore.ContentSearch.SearchTypes;

namespace Sitecore.ContentSearch.Spatial.Provider.Lucene
{
    public class LuceneSearchWithSpatialContext:LuceneSearchContext
    {
        private  IContentSearchConfigurationSettings settings;


        protected LuceneSearchWithSpatialContext(ILuceneProviderIndex index, IEnumerable<ILuceneProviderSearchable> searchables, LuceneIndexAccess indexAccess = LuceneIndexAccess.ReadOnly, SearchSecurityOptions securityOptions = SearchSecurityOptions.EnableSecurityCheck)
            : base(index, searchables, LuceneIndexAccess.ReadOnly, securityOptions)
        {
            this.settings = index.Locator.GetInstance<IContentSearchConfigurationSettings>();
        }

        public LuceneSearchWithSpatialContext(ILuceneProviderIndex index, SearchSecurityOptions securityOptions = SearchSecurityOptions.EnableSecurityCheck)
            :base(index,securityOptions)
        {
            this.settings = index.Locator.GetInstance<IContentSearchConfigurationSettings>();
        }

        public override IQueryable<TItem> GetQueryable<TItem>()
        {
            return this.GetQueryable<TItem>(new IExecutionContext[0]);

        }

        public override IQueryable<TItem> GetQueryable<TItem>(IExecutionContext executionContext)
        {
            return this.GetQueryable<TItem>(new IExecutionContext[] { executionContext });
        }


        public override IQueryable<TItem> GetQueryable<TItem>(params IExecutionContext[] executionContexts)
        {
            LinqToLuceneIndexWithSpatial<TItem> linqToLuceneIndex = new LinqToLuceneIndexWithSpatial<TItem>(this, executionContexts);
            if (settings.EnableSearchDebug())
            {
                ((IHasTraceWriter)linqToLuceneIndex).TraceWriter = new LoggingTraceWriter(SearchLog.Log);
            }
            IQueryable<TItem> queryable = linqToLuceneIndex.GetQueryable();
            if (typeof(SearchResultItem).IsAssignableFrom(typeof(TItem)))
            {
                QueryGlobalFiltersArgs queryGlobalFiltersArg = new QueryGlobalFiltersArgs(linqToLuceneIndex.GetQueryable(), typeof(TItem), executionContexts.ToList<IExecutionContext>());
                this.Index.Locator.GetInstance<ICorePipeline>().Run("contentSearch.getGlobalLinqFilters", queryGlobalFiltersArg);
                queryable = (IQueryable<TItem>)queryGlobalFiltersArg.Query;
            }
            return queryable;
        }

        protected virtual void Dispose(bool disposing)
        {
            base.Dispose();
        }

        public override void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}