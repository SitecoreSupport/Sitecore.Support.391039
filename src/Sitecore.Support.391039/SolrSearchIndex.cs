namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using Sitecore.ContentSearch.Security;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Maintenance;

    public class SolrSearchIndex : Sitecore.ContentSearch.SolrProvider.SolrSearchIndex, ISearchIndex
    {
        protected internal ConnectionStatus PreviousConnectionStatus = ConnectionStatus.Unknown;

        public override IProviderSearchContext CreateSearchContext(SearchSecurityOptions options = SearchSecurityOptions.Default)
        {
            if (this.Group == IndexGroup.Experience)
            {
                return new Sitecore.Support.ContentSearch.SolrProvider.SolrAnalyticsSearchContext(this, options);
            }
            return new Sitecore.Support.ContentSearch.SolrProvider.SolrSearchContext(this, options);
        }

        public SolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore) : this(name, core, propertyStore, null)
        {
        }

        public SolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore, string group) : base(name, core, propertyStore, group)
        {

        }

        void ISearchIndex.Initialize()
        {
            SolrStatusMonitor.CheckCoreStatus(this);
            if (this.PreviousConnectionStatus == ConnectionStatus.Succeded)
            {
                base.Initialize();
            }
        }
    }
}

