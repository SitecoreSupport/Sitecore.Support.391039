using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentSearch.Security;

namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using Sitecore.ContentSearch;
    public class SwitchOnRebuildSolrSearchIndex : Sitecore.ContentSearch.SolrProvider.SwitchOnRebuildSolrSearchIndex, ISearchIndex
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

        public SwitchOnRebuildSolrSearchIndex(string name, string core, string rebuildcore, IIndexPropertyStore propertyStore) : base(name, core, rebuildcore, propertyStore)
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
