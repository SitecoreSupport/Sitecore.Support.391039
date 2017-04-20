
namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using Sitecore.ContentSearch.Security;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Maintenance;

    public class SolrSearchIndex : Sitecore.ContentSearch.SolrProvider.SolrSearchIndex, IFailResistantIndex
    {
        protected internal ConnectionStatus PreviousConnectionStatus = ConnectionStatus.Unknown;

        public override IProviderSearchContext CreateSearchContext(SearchSecurityOptions options = SearchSecurityOptions.Default)
        {
            return new Sitecore.Support.ContentSearch.SolrProvider.SolrSearchContext(this, options);
        }

        public SolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore) : this(name, core, propertyStore, null)
        {
        }

        public SolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore, string group) : base(name, core, propertyStore, group)
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

