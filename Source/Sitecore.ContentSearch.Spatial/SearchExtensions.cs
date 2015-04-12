using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch.Spatial.Indexing;
using Sitecore.ContentSearch.Spatial.Provider.Lucene;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Utilities;
using Sitecore.ContentSearch.Diagnostics;
using Sitecore.ContentSearch.LuceneProvider;

namespace Sitecore.ContentSearch.Spatial
{
    public static  class SearchExtensions
    {

        public static IQueryable<TResult> GetExtendedQueryable<TResult>(this IProviderSearchContext context)
        {
            return GetExtendedQueryable<TResult>(context, null); 
        }

        public static IQueryable<TResult> GetExtendedQueryable<TResult>(this IProviderSearchContext context, params IExecutionContext[] executionContext)
        
        {
            IQueryable<TResult> queryable;
            var luceneContext = context as LuceneSearchWithSpatialContext;
            if (luceneContext != null)
            {
                queryable = GetLuceneQueryable<TResult>(luceneContext, executionContext);
            }
            else
            {
                throw new NotImplementedException("Current Index is not configured to use Spatial Search.");
            }
            ;
            return queryable;
        }

        private static IQueryable<TResult> GetLuceneQueryable<TResult>(LuceneSearchWithSpatialContext context, IExecutionContext[] executionContext)
        {
            var linqToLuceneIndex = new LinqToLuceneIndexWithSpatial<TResult>(context, executionContext);
            if (context.Index.Locator.GetInstance<IContentSearchConfigurationSettings>().EnableSearchDebug())
                ((IHasTraceWriter)linqToLuceneIndex).TraceWriter = new LoggingTraceWriter(SearchLog.Log);
            return linqToLuceneIndex.GetQueryable();
        }

    }
}