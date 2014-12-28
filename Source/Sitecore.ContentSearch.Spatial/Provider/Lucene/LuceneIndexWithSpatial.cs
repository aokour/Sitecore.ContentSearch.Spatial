using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.LuceneProvider;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.Security;
using System.Threading;
using Lucene.Net.Search;
using Sitecore.Data;
using Sitecore.SecurityModel;
using Sitecore.Data.Items;
using Sitecore.ContentSearch.Spatial.Configurations;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Xml;
using Sitecore.ContentSearch.Spatial.Query;
using Lucene.Net.Spatial.Util;
using Spatial4n.Core.Distance;
using Lucene.Net.Spatial.Queries;
using Spatial4n.Core.Shapes;
using Spatial4n.Core.Context;
using Lucene.Net.Spatial.Vector;
using System.IO;
using Sitecore.IO;
using Lucene.Net.Store;

using Lucene.Net.Index;

namespace Sitecore.ContentSearch.Spatial.Provider.Lucene
{
    public class LuceneIndexWithSpatial:LuceneIndex
    {
        private static SpatialConfigurations spatialConfigurations;
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