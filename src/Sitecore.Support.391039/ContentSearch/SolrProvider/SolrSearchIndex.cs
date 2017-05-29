using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Support.ContentSearch.SolrProvider
{

    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Maintenance;
    using Sitecore.ContentSearch.SolrProvider;
    using Sitecore.Diagnostics;
    using SolrNet;
    using SolrNet.Exceptions;
    using System;

    public class SolrSearchIndex : Sitecore.ContentSearch.SolrProvider.SolrSearchIndex, ISearchIndex, IDisposable
    {
        public SolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore) : base(name, core, propertyStore)
        {
        }

        public SolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore, string group) : base(name, core, propertyStore, group)
        {
        }

        protected internal static bool IsOkSolrConnection()
        {
            try
            {
                ISolrCoreAdmin solrAdmin = SolrContentSearchManager.SolrAdmin;
                if (solrAdmin != null)
                {
                    solrAdmin.Status();
                }
                return true;
            }
            catch (SolrConnectionException)
            {
                Log.Warn((("SUPPORT: Unable to connect to Solr: " + SolrContentSearchManager.ServiceAddress) ?? "") + "; " + typeof(SolrConnectionException).FullName + " was caught.", new object());
                return false;
            }
        }

        void ISearchIndex.Initialize()
        {
            if (IsOkSolrConnection())
            {
                base.Initialize();
            }
        }
    }

}