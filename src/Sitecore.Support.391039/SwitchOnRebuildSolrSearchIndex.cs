﻿
namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Maintenance;
    using Sitecore.ContentSearch.Security;

    public class SwitchOnRebuildSolrSearchIndex : Sitecore.ContentSearch.SolrProvider.SwitchOnRebuildSolrSearchIndex, IFailResistantIndex
    {
        protected internal ConnectionStatus PreviousConnectionStatus = ConnectionStatus.Unknown;
        public override IProviderSearchContext CreateSearchContext(SearchSecurityOptions options = SearchSecurityOptions.Default)
        {
            return new Sitecore.Support.ContentSearch.SolrProvider.SolrSearchContext(this, options);
        }

        public SwitchOnRebuildSolrSearchIndex(string name, string core, string rebuildcore, IIndexPropertyStore propertyStore) : base(name, core, rebuildcore, propertyStore)
        {
        }

        public override void Initialize()
        {
            // SolrStatusMonitor triggers index intialization 
            SolrStatusMonitor.CheckCoreStatus(this);
        }

        ConnectionStatus IFailResistantIndex.ConnectionStatus
        {
            get
            {
                return this.PreviousConnectionStatus;
            }

            set
            {
                this.PreviousConnectionStatus = value;
            }
        }
        
        void IFailResistantIndex.Init()
        {
            base.Initialize();
        }
    }
}
