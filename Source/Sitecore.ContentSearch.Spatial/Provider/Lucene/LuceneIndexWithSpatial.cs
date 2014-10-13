using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.LuceneProvider;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.Security;

namespace Sitecore.ContentSearch.Spatial.Provider.Lucene
{
    public class LuceneIndexWithSpatial:LuceneIndex
    {
        public LuceneIndexWithSpatial(string name, string folder, IIndexPropertyStore propertyStore)
            :base(name,folder,propertyStore)
        {

        }

        public override IProviderSearchContext CreateSearchContext(SearchSecurityOptions securityOptions = SearchSecurityOptions.EnableSecurityCheck)
        {
            this.EnsureInitialized();
            return new LuceneSearchWithSpatialContext(this, securityOptions);
        }

    }
}