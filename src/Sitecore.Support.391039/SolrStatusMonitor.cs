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
            if (solrAdmin != null)
            {
                var solrSearchIndex = index as SolrSearchIndex;
                if (solrSearchIndex != null)
                {
                    try
                    {
                        var coreResult = solrAdmin.Status(solrSearchIndex.Core).FirstOrDefault();
                        if (coreResult.Index == null)
                        {
                            throw new SolrConnectionException("SUPPORT: Core's index is null.");
                        }
                        var failResistantSolrSearchIndex2 = (solrSearchIndex as Sitecore.Support.ContentSearch.SolrProvider.SolrSearchIndex);
                        if (failResistantSolrSearchIndex2 != null)
                        {
                            if (failResistantSolrSearchIndex2.PreviousConnectionStatus == ConnectionStatus.Unknown)
                            {
                                Log.Info("SUPPORT: Connection to [" + failResistantSolrSearchIndex2.Core + "] Solr core was established. [" + failResistantSolrSearchIndex2.Name + "] index is being initialized.", failResistantSolrSearchIndex2);
                                failResistantSolrSearchIndex2.Initialize();
                            }
                            else if (failResistantSolrSearchIndex2.PreviousConnectionStatus == ConnectionStatus.Failed)
                            {
                                Log.Info("SUPPORT: Connection to [" + failResistantSolrSearchIndex2.Core + "] Solr core was restored.", failResistantSolrSearchIndex2);
                            }
                            failResistantSolrSearchIndex2.PreviousConnectionStatus = ConnectionStatus.Succeded;
                        }
                        else
                        {
                            var failResistantSwitchOnRebuildSolrSearchIndex = (solrSearchIndex as Sitecore.Support.ContentSearch.SolrProvider.SwitchOnRebuildSolrSearchIndex);
                            if (failResistantSwitchOnRebuildSolrSearchIndex != null)
                            {
                                if (failResistantSwitchOnRebuildSolrSearchIndex.PreviousConnectionStatus == ConnectionStatus.Unknown)
                                {
                                    Log.Info("SUPPORT: Connection to [" + failResistantSwitchOnRebuildSolrSearchIndex.Core + "] Solr core was established. [" + failResistantSwitchOnRebuildSolrSearchIndex.Name + "] index is being initialized.", failResistantSwitchOnRebuildSolrSearchIndex);
                                    failResistantSwitchOnRebuildSolrSearchIndex.Initialize();
                                }
                                else if (failResistantSwitchOnRebuildSolrSearchIndex.PreviousConnectionStatus == ConnectionStatus.Failed)
                                {
                                    Log.Info("SUPPORT: Connection to [" + failResistantSwitchOnRebuildSolrSearchIndex.Core + "] Solr core was restored.", failResistantSwitchOnRebuildSolrSearchIndex);
                                }
                                failResistantSwitchOnRebuildSolrSearchIndex.PreviousConnectionStatus = ConnectionStatus.Succeded;
                            }
                        }
                    }
                    catch (SolrConnectionException ex)
                    {
                        if (ex.Message.Contains("java.lang.IllegalStateException") && ex.Message.Contains("appears both in delegate and in cache"))
                        {
                            Log.Warn("SUPPORT: Status check for [" + solrSearchIndex.Core + "] Solr core failed. Error suppressed as not related to Solr core availability. Details: https://issues.apache.org/jira/browse/LUCENE-7188", ex);
                            return;
                        }
                        Log.Warn("SUPPORT: Unable to connect to [" + SolrContentSearchManager.ServiceAddress + "], Core: [" + solrSearchIndex.Core + "]", ex, solrSearchIndex);
                        var failResistantSolrSearchIndex = (solrSearchIndex as Sitecore.Support.ContentSearch.SolrProvider.SolrSearchIndex);
                        if (failResistantSolrSearchIndex != null)
                        {
                            if (failResistantSolrSearchIndex.PreviousConnectionStatus == ConnectionStatus.Succeded)
                            {
                                Log.Warn("SUPPORT: Connection to [" + failResistantSolrSearchIndex.Core + "] Solr core was lost.", failResistantSolrSearchIndex);
                                failResistantSolrSearchIndex.PreviousConnectionStatus = ConnectionStatus.Failed;
                            }
                            else if (failResistantSolrSearchIndex.PreviousConnectionStatus == ConnectionStatus.Unknown)
                            {
                                Log.Warn("SUPPORT: Connection to [" + failResistantSolrSearchIndex.Core + "] Solr core was not established.", failResistantSolrSearchIndex);
                            }                            
                        }
                        else
                        {
                            var failResistantSwitchOnRebuildSolrSearchIndex = (solrSearchIndex as Sitecore.Support.ContentSearch.SolrProvider.SwitchOnRebuildSolrSearchIndex);
                            if (failResistantSwitchOnRebuildSolrSearchIndex != null)
                            {
                                if (failResistantSwitchOnRebuildSolrSearchIndex.PreviousConnectionStatus == ConnectionStatus.Succeded)
                                {
                                    Log.Warn("SUPPORT: Connection to [" + failResistantSwitchOnRebuildSolrSearchIndex.Core + "] Solr core was lost.", failResistantSwitchOnRebuildSolrSearchIndex);
                                    failResistantSwitchOnRebuildSolrSearchIndex.PreviousConnectionStatus = ConnectionStatus.Failed;
                                }
                                else if (failResistantSwitchOnRebuildSolrSearchIndex.PreviousConnectionStatus == ConnectionStatus.Unknown)
                                {
                                    Log.Warn("SUPPORT: Connection to [" + failResistantSwitchOnRebuildSolrSearchIndex.Core + "] Solr core was not established.", failResistantSwitchOnRebuildSolrSearchIndex);
                                }                                
                            }
                        }
                        return;
                    }
                }
            }
        }

        private static void CheckSolrStatus(object sender, EventArgs args)
        {
            ISolrCoreAdmin solrAdmin = SolrContentSearchManager.SolrAdmin;
            if (solrAdmin != null)
            {
                try
                {
                    var coreResult = solrAdmin.Status().FirstOrDefault();
                }
                catch (SolrConnectionException ex)
                {
                    if (ex.Message.Contains("java.lang.IllegalStateException") && ex.Message.Contains("appears both in delegate and in cache"))
                    {
                        Log.Warn("SUPPORT: Status check for [" + SolrContentSearchManager.ServiceAddress + "] Solr server failed. Error suppressed as not related to Solr core availability. Details: https://issues.apache.org/jira/browse/LUCENE-7188", ex);
                        return;
                    }
                    Log.Warn("SUPPORT: Unable to connect to [" + SolrContentSearchManager.ServiceAddress + "]. All Solr search indexes are unavailable.", ex, solrAdmin);
                    foreach (var index in SolrContentSearchManager.Indexes)
                    {
                        var failResistantSolrSearchIndex = index as Sitecore.Support.ContentSearch.SolrProvider.SolrSearchIndex;
                        if (failResistantSolrSearchIndex != null && failResistantSolrSearchIndex.PreviousConnectionStatus != ConnectionStatus.Unknown)
                        {
                            failResistantSolrSearchIndex.PreviousConnectionStatus = ConnectionStatus.Failed;
                            continue;
                        }
                        var failResistantSwitchOnRebuildSolrSearchIndex = index as Sitecore.Support.ContentSearch.SolrProvider.SwitchOnRebuildSolrSearchIndex;
                        if (failResistantSwitchOnRebuildSolrSearchIndex != null && failResistantSwitchOnRebuildSolrSearchIndex.PreviousConnectionStatus != ConnectionStatus.Unknown)
                        {
                            failResistantSwitchOnRebuildSolrSearchIndex.PreviousConnectionStatus = ConnectionStatus.Failed;
                            continue;
                        }
                    }
                    return;
                }
                foreach (var index in SolrContentSearchManager.Indexes)
                {
                    CheckCoreStatus(index);
                }
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

