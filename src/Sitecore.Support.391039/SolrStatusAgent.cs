using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Support
{
    using Sitecore.ContentSearch.SolrProvider;
    using Sitecore.Diagnostics;
    using Sitecore.Support.ContentSearch.SolrProvider;
    using Sitecore.Tasks;
    using System;

    public class SolrStatusAgent : BaseAgent
    {
        private static ConnectionStatus prevConnectionStatus;

        public void Run()
        {
            bool flag = Sitecore.Support.ContentSearch.SolrProvider.SolrSearchIndex.IsOkSolrConnection();
            if ((prevConnectionStatus == ConnectionStatus.Never) & flag)
            {
                Log.Warn("SUPPORT: SOLR connection was restored. Indexes are being initialized.", this);
                SolrContentSearchManager.Initialize();
                prevConnectionStatus = ConnectionStatus.Ok;
            }
            else if ((prevConnectionStatus == ConnectionStatus.No) & flag)
            {
                Log.Warn("SUPPORT: SOLR connection was restored.", this);
                prevConnectionStatus = ConnectionStatus.Ok;
            }
            else if ((prevConnectionStatus == ConnectionStatus.Ok) && !flag)
            {
                prevConnectionStatus = ConnectionStatus.No;
            }
        }

        private enum ConnectionStatus
        {
            Never,
            No,
            Ok
        }
    }


}