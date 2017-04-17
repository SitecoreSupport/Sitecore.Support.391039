namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Diagnostics;
    using Sitecore.ContentSearch.Linq.Common;
    using Sitecore.ContentSearch.Pipelines.QueryGlobalFilters;
    using Sitecore.ContentSearch.SearchTypes;
    using Sitecore.ContentSearch.Security;
    using Sitecore.ContentSearch.Utilities;
    using Sitecore.Diagnostics;

    public class SolrSearchContext : Sitecore.ContentSearch.SolrProvider.SolrSearchContext, IProviderSearchContext
    {
        private readonly IContentSearchConfigurationSettings contentSearchSettings;

        private readonly Sitecore.ContentSearch.SolrProvider.SolrSearchIndex index;

        public SolrSearchContext(Sitecore.ContentSearch.SolrProvider.SolrSearchIndex solrSearchIndex, SearchSecurityOptions options) : base(solrSearchIndex, options)
        {
            this.index = solrSearchIndex;
            this.contentSearchSettings = this.Index.Locator.GetInstance<IContentSearchConfigurationSettings>();
        }

        IQueryable<TItem> IProviderSearchContext.GetQueryable<TItem>()
        {
           return this.GetQueryableImpl<TItem>(new IExecutionContext[0]);
        }

        IQueryable<TItem> IProviderSearchContext.GetQueryable<TItem>(IExecutionContext executionContext)
        {
            return this.GetQueryableImpl<TItem>( new[] {executionContext} );
        }

        IQueryable<TItem> IProviderSearchContext.GetQueryable<TItem>(params IExecutionContext[] executionContexts)
        {
            return this.GetQueryableImpl<TItem>(executionContexts);
        }

        protected virtual IQueryable<TItem> GetQueryableImpl<TItem>(params IExecutionContext[] executionContexts)
        {
            var failResistantIndex = this.Index as IFailResistantIndex;

            if (failResistantIndex != null)
            {
                if (failResistantIndex.ConnectionStatus != ConnectionStatus.Succeded)
                {
                    Log.Error("SUPPORT: unable to execute a search query. Solr core [" + this.index.Core + "] is unavailable.", this);
                    return new EnumerableQuery<TItem>(new List<TItem>());
                }
            }

            LinqToSolrIndex<TItem> linqToSolrIndex = new Sitecore.Support.ContentSearch.SolrProvider.LinqToSolrIndex<TItem>(this, executionContexts);

            if (this.contentSearchSettings.EnableSearchDebug())
            {
                ((IHasTraceWriter)linqToSolrIndex).TraceWriter = new LoggingTraceWriter(SearchLog.Log);
            }

            IQueryable<TItem> result = linqToSolrIndex.GetQueryable();
            if (typeof(TItem).IsAssignableFrom(typeof(SearchResultItem)))
            {
                QueryGlobalFiltersArgs queryGlobalFiltersArgs = new QueryGlobalFiltersArgs(linqToSolrIndex.GetQueryable(), typeof(TItem), executionContexts.ToList<IExecutionContext>());
                this.Index.Locator.GetInstance<Sitecore.Abstractions.ICorePipeline>().Run("contentSearch.getGlobalLinqFilters", queryGlobalFiltersArgs);
                result = (IQueryable<TItem>)queryGlobalFiltersArgs.Query;
            }

            return result;
        }
    }
}
