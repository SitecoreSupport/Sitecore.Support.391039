namespace Sitecore.Support
{
    using Sitecore.Configuration;
    using Sitecore.Events.Hooks;
    using Sitecore.Services;
    using Sitecore.ContentSearch.SolrProvider;
    using Sitecore.Diagnostics;
    using System;
    using System.Linq;
    using Sitecore.ContentSearch;
    using SolrNet;
    using SolrNet.Exceptions;

    public class SolrStatusMonitor : IHook
    {
        private static AlarmClock _alarmClock;

        internal static void CheckCoreStatus(ISearchIndex index)
        {
            ISolrCoreAdmin solrAdmin = SolrContentSearchManager.SolrAdmin;
            if (solrAdmin == null)
            {
                return;
            }

            var solrSearchIndex = index as Sitecore.ContentSearch.SolrProvider.SolrSearchIndex;
            if (solrSearchIndex == null)
            {
                return;
            }

            var failResistantIndex = index as IFailResistantIndex;

            if (failResistantIndex == null)
            {
                // the index doesn't support this feature, so the index is considered as always available
                return;
            }

            try
            {
                var newStatus = solrAdmin.Status(solrSearchIndex.Core).FirstOrDefault();

                if (newStatus.Index == null)
                {
                    throw new SolrConnectionException("SUPPORT: Core's index is null.");
                }

                if (failResistantIndex.ConnectionStatus == ConnectionStatus.Unknown)
                {
                    Log.Info(
                        $"SUPPORT: Connection to [{solrSearchIndex.Core}] Solr core was established. [{solrSearchIndex.Name}] index is being initialized.",
                        solrSearchIndex);
                    failResistantIndex.Init();
                }
                else if (failResistantIndex.ConnectionStatus == ConnectionStatus.Failed)
                {
                    Log.Info($"SUPPORT: Connection to [{solrSearchIndex.Core}] Solr core was restored.", solrSearchIndex);
                }

                failResistantIndex.ConnectionStatus = ConnectionStatus.Succeded;
            }
            catch (SolrConnectionException ex)
            {
                if (ex.Message.Contains("java.lang.IllegalStateException") && ex.Message.Contains("appears both in delegate and in cache"))
                {
                    Log.Warn(
                        $"SUPPORT: Status check for [{solrSearchIndex.Core}] Solr core failed. Error suppressed as not related to Solr core availability. Details: https://issues.apache.org/jira/browse/LUCENE-7188",
                        solrSearchIndex);
                    Log.Debug($"SUPPORT: Solr exception\r\n{ex}", solrAdmin);

                    return;
                }

                Log.Warn($"SUPPORT: Unable to connect to [{SolrContentSearchManager.ServiceAddress}], Core: [{solrSearchIndex.Core}]",
                    ex, solrSearchIndex);

                if (failResistantIndex.ConnectionStatus == ConnectionStatus.Succeded)
                {
                    Log.Warn($"SUPPORT: Connection to [{solrSearchIndex.Core}] Solr core was lost.", failResistantIndex);

                    failResistantIndex.ConnectionStatus = ConnectionStatus.Failed;
                }
                else if (failResistantIndex.ConnectionStatus == ConnectionStatus.Unknown)
                {
                    Log.Warn($"SUPPORT: Connection to [{solrSearchIndex.Core}] Solr core was not established.", failResistantIndex);
                }
            }
        }

        private static void CheckSolrStatus(object sender, EventArgs args)
        {
            ISolrCoreAdmin solrAdmin = SolrContentSearchManager.SolrAdmin;
            if (solrAdmin == null)
            {
                return;
            }

            try
            {
                var coreResult = solrAdmin.Status().FirstOrDefault();
            }
            catch (SolrConnectionException ex)
            {
                if (ex.Message.Contains("java.lang.IllegalStateException") && ex.Message.Contains("appears both in delegate and in cache"))
                {
                    Log.Warn($"SUPPORT: Status check for [{SolrContentSearchManager.ServiceAddress}] Solr server failed. Error suppressed as not related to Solr core availability. Details: https://issues.apache.org/jira/browse/LUCENE-7188", solrAdmin);
                    Log.Debug($"SUPPORT: Solr exception\r\n{ex}", solrAdmin);

                    return;
                }

                Log.Warn($"SUPPORT: Unable to connect to [{SolrContentSearchManager.ServiceAddress}]. All Solr search indexes are unavailable.", ex, solrAdmin);
                Log.Debug($"SUPPORT: Solr exception\r\n{ex}", solrAdmin);

                foreach (var index in SolrContentSearchManager.Indexes)
                {
                    var resistantIndex = index as IFailResistantIndex;
                    if (resistantIndex != null && resistantIndex.ConnectionStatus != ConnectionStatus.Unknown)
                    {
                        resistantIndex.ConnectionStatus = ConnectionStatus.Failed;
                    }
                }

                return;
            }

            foreach (var index in SolrContentSearchManager.Indexes)
            {
                CheckCoreStatus(index);
            }
        }

        public void Initialize()
        {
            TimeSpan updateInterval = Settings.GetTimeSpanSetting("ContentSearch.SolrStatusMonitor.Interval", TimeSpan.FromSeconds(10));
            _alarmClock = new AlarmClock(updateInterval);
            _alarmClock.Ring += CheckSolrStatus;
        }
    }
}

