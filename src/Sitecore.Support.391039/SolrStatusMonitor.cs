namespace Sitecore.Support
{
    using Sitecore.Configuration;
    using Sitecore.Events.Hooks;
    using Sitecore.Services;
    using Sitecore.ContentSearch.SolrProvider;
    using Sitecore.Diagnostics;
    using System;
    using System.Linq;
    using SolrNet;
    using SolrNet.Exceptions;

    public class SolrStatusMonitor : IHook
    {
        protected AlarmClock alarmClock;
        
        protected virtual void CheckSolrStatus(object sender, EventArgs args)
        {
            ISolrCoreAdmin solrAdmin = SolrContentSearchManager.SolrAdmin;
            if (solrAdmin == null)
            {
                return;
            }

            var resistantIndexes = this.GetResistantIndexes();

            if (resistantIndexes.Length == 0)
            {
                // Nothing to check, the feature is not used
                return;
            }

            try
            {
                var status = solrAdmin.Status().FirstOrDefault();
            }
            catch (SolrConnectionException ex)
            {
                if (ex.Message.Contains("java.lang.IllegalStateException") && ex.Message.Contains("appears both in delegate and in cache"))
                {
                    Log.Warn($"SUPPORT: Status check for [{SolrContentSearchManager.ServiceAddress}] Solr server failed. Error suppressed as not related to Solr core availability. Details: https://issues.apache.org/jira/browse/LUCENE-7188", solrAdmin);

                    return;
                }

                Log.Warn("SUPPORT: Solr Connection Error", ex, this);

                this.SetAllIndexStatusToFail(resistantIndexes);

                return;
            }

            this.CheckIndexStatusSeparately(resistantIndexes);
        }

        protected virtual IFailResistantIndex[] GetResistantIndexes()
        {
            var allIndexes = SolrContentSearchManager.Indexes.ToArray();

            return allIndexes.Where(indx => indx is IFailResistantIndex).Cast<IFailResistantIndex>().ToArray();
        }

        protected virtual void SetAllIndexStatusToFail(IFailResistantIndex[] indexes)
        {
            Log.Warn($"SUPPORT: Unable to connect to [{SolrContentSearchManager.ServiceAddress}]. All Solr search indexes are unavailable.", this);

            foreach (var resistantIndex in indexes)
            {
                if (resistantIndex.ConnectionStatus != ConnectionStatus.Unknown)
                {
                    resistantIndex.SetStatus(ConnectionStatus.Failed);
                }
            }
        }

        protected virtual void CheckIndexStatusSeparately(IFailResistantIndex[] indexes)
        {
            foreach (var resistantIndex in indexes)
            {
                var curStatus = resistantIndex.ConnectionStatus;

                var newStatus = resistantIndex.CheckStatus();

                if (curStatus == ConnectionStatus.Succeded && newStatus == ConnectionStatus.Failed)
                {
                    Log.Warn($"SUPPORT: [Index={resistantIndex.Name}] Connection is lost...", this);
                    resistantIndex.SetStatus(ConnectionStatus.Failed);
                    continue;
                }

                if (curStatus == ConnectionStatus.Failed && newStatus == ConnectionStatus.Succeded)
                {
                    Log.Warn($"SUPPORT: [Index={resistantIndex.Name}] Connection is restored...", this);
                    resistantIndex.SetStatus(ConnectionStatus.Succeded);
                    continue;
                }

                // First successful connection to a search server
                if (curStatus == ConnectionStatus.Unknown && newStatus == ConnectionStatus.Succeded)
                {
                    Log.Warn($"SUPPORT: [Index={resistantIndex.Name}] Completing index initialization...", this);
                    resistantIndex.Connect();
                    continue;
                }
            }
        }

        public void Initialize()
        {
            TimeSpan updateInterval = Settings.GetTimeSpanSetting("ContentSearch.SolrStatusMonitor.Interval", TimeSpan.FromSeconds(10));
            alarmClock = new AlarmClock(updateInterval);
            alarmClock.Ring += CheckSolrStatus;
        }
    }
}
