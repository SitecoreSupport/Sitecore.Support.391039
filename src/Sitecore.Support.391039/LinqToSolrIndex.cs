namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using System.Collections.Generic;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Linq.Common;
    using Sitecore.ContentSearch.Linq.Solr;
    using Sitecore.Diagnostics;

    public class LinqToSolrIndex<TItem> : Sitecore.ContentSearch.SolrProvider.LinqToSolrIndex<TItem>
    {
        private readonly Sitecore.Support.ContentSearch.SolrProvider.SolrSearchContext context;

        public LinqToSolrIndex(Sitecore.Support.ContentSearch.SolrProvider.SolrSearchContext context, IExecutionContext executionContext) : base(context, executionContext)
        {
            this.context = context;
        }

        public LinqToSolrIndex(Sitecore.Support.ContentSearch.SolrProvider.SolrSearchContext context, IExecutionContext[] executionContexts) : base(context, executionContexts)
        {
            this.context = context;
        }

        public override IEnumerable<TElement> FindElements<TElement>(SolrCompositeQuery compositeQuery)
        {
            if (this.IsIndexAvailable(this.context.Index))
            {
                return base.FindElements<TElement>(compositeQuery);
            }

            var solrIndex = this.context.Index as Sitecore.ContentSearch.SolrProvider.SolrSearchIndex;
            Log.Error("SUPPORT: unable to execute a search query. Solr core [" + solrIndex.Core + "] is unavailable.", this);
            return new System.Collections.Generic.List<TElement>();
        }

        public override TResult Execute<TResult>(SolrCompositeQuery compositeQuery)
        {
            if (this.IsIndexAvailable(this.context.Index))
            {
                return base.Execute<TResult>(compositeQuery);
            }

            var solrIndex = this.context.Index as Sitecore.ContentSearch.SolrProvider.SolrSearchIndex;
            Log.Error("SUPPORT: unable to execute a search query. Solr core [" + solrIndex.Core + "] is unavailable.", this);

            return default(TResult);
        }

        protected virtual bool IsIndexAvailable(ISearchIndex index)
        {
            var failResistantIndex = this.context.Index as IFailResistantIndex;

            if (failResistantIndex == null)
            {
                // the index doesn't support this feature, so it is considered the index is always available
                return true;
            }

            if (failResistantIndex.ConnectionStatus == ConnectionStatus.Succeded)
            {
                return true;
            }

            return false;
        }
    }
}
