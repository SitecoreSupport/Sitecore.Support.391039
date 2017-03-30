namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using Sitecore.ContentSearch.Security;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Maintenance;

    public class SolrSearchIndex : Sitecore.ContentSearch.SolrProvider.SolrSearchIndex, ISearchIndex
    {
        protected internal ConnectionStatus PreviousConnectionStatus = ConnectionStatus.Unknown;

        public override IProviderSearchContext CreateSearchContext(SearchSecurityOptions options = SearchSecurityOptions.EnableSecurityCheck)
        {
            return new Sitecore.Support.ContentSearch.SolrProvider.SolrSearchContext(this, options);
        }
    
        public SolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore) : base(name, core, propertyStore)
        {

        }

        void ISearchIndex.Initialize()
        {
            SolrStatusMonitor.CheckCoreStatus(this);            
        }
    }
}

