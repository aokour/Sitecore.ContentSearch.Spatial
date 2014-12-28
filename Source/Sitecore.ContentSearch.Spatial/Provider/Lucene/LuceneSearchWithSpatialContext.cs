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

namespace Sitecore.ContentSearch.Spatial.Provider.Lucene
{
    public class LuceneSearchWithSpatialContext:LuceneSearchContext
    {
        private  ILuceneProviderIndex index;
     
        private  IContentSearchConfigurationSettings settings;


        protected LuceneSearchWithSpatialContext(ILuceneProviderIndex index, CreateSearcherOption options = CreateSearcherOption.Writeable, SearchSecurityOptions securityOptions = SearchSecurityOptions.EnableSecurityCheck)
            :base(index,options,securityOptions)
        {
            Assert.ArgumentNotNull(index, "index");
            this.index = index;
            this.settings = this.index.Locator.GetInstance<IContentSearchConfigurationSettings>();
        }

        public LuceneSearchWithSpatialContext(ILuceneProviderIndex index, SearchSecurityOptions securityOptions = SearchSecurityOptions.EnableSecurityCheck)
            :base(index,securityOptions)
        {
            Assert.ArgumentNotNull(index, "index");
            this.index = index;
            this.settings = this.index.Locator.GetInstance<IContentSearchConfigurationSettings>();
        }

        public new IQueryable<TItem> GetQueryable<TItem>()
        {
            return this.GetQueryable<TItem>(new IExecutionContext[0]);

        }

        public new IQueryable<TItem> GetQueryable<TItem>(IExecutionContext executionContext)
        {
            return this.GetQueryable<TItem>(new IExecutionContext[] { executionContext });
        }


        public new IQueryable<TItem> GetQueryable<TItem>(params IExecutionContext[] executionContexts)
        {
            LinqToLuceneIndexWithSpatial<TItem> linqToLuceneIndex = new LinqToLuceneIndexWithSpatial<TItem>(this, executionContexts);
            if (settings.EnableSearchDebug())
            {
                ((IHasTraceWriter)linqToLuceneIndex).TraceWriter = new LoggingTraceWriter(SearchLog.Log);
            }
            QueryGlobalFiltersArgs args = new QueryGlobalFiltersArgs(linqToLuceneIndex.GetQueryable(), typeof(TItem), executionContexts.ToList<IExecutionContext>());
            this.Index.Locator.GetInstance<ICorePipeline>().Run("contentSearch.getGlobalLinqFilters", args);
            return (IQueryable<TItem>)args.Query;
        }

       

    }
}