namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using Sitecore.ContentSearch.Diagnostics;
    using Sitecore.ContentSearch.Linq.Common;
    using Sitecore.ContentSearch.Pipelines.QueryGlobalFilters;
    using Sitecore.ContentSearch.SearchTypes;
    using Sitecore.ContentSearch.Security;
    using Sitecore.Diagnostics;

    public class SolrSearchContext : Sitecore.ContentSearch.SolrProvider.SolrSearchContext, Sitecore.ContentSearch.IProviderSearchContext
    {
        public SolrSearchContext(Sitecore.ContentSearch.SolrProvider.SolrSearchIndex solrSearchIndex, SearchSecurityOptions options) : base(solrSearchIndex, options)
        {
        }

        IQueryable<TItem> Sitecore.ContentSearch.IProviderSearchContext.GetQueryable<TItem>()
        {
            return ((Sitecore.ContentSearch.IProviderSearchContext)this).GetQueryable<TItem>(new IExecutionContext[0]);
        }

        IQueryable<TItem> Sitecore.ContentSearch.IProviderSearchContext.GetQueryable<TItem>(IExecutionContext executionContext)
        {
            return ((Sitecore.ContentSearch.IProviderSearchContext)this).GetQueryable<TItem>(new IExecutionContext[]
            {
                executionContext
            });
        }

        IQueryable<TItem> Sitecore.ContentSearch.IProviderSearchContext.GetQueryable<TItem>(params IExecutionContext[] executionContexts)
        {
            var failResistantSolrSearchIndex = this.Index as Sitecore.Support.ContentSearch.SolrProvider.SolrSearchIndex;
            if (failResistantSolrSearchIndex != null && failResistantSolrSearchIndex.PreviousConnectionStatus != ConnectionStatus.Succeded)
            { 
                Log.Error("SUPPORT: unable to execute a search query. Solr core ["+ failResistantSolrSearchIndex.Core + "] is unavailable.", typeof(SolrSearchContext));
                return new EnumerableQuery<TItem>(new List<TItem>());
            }
            var failResistantSwitchOnRebuildSolrSearchIndex = this.Index as Sitecore.Support.ContentSearch.SolrProvider.SwitchOnRebuildSolrSearchIndex;
            if (failResistantSwitchOnRebuildSolrSearchIndex != null && failResistantSwitchOnRebuildSolrSearchIndex.PreviousConnectionStatus != ConnectionStatus.Succeded)
            {
                Log.Error("SUPPORT: unable to execute a search query. Solr core [" + failResistantSwitchOnRebuildSolrSearchIndex.Core + "] is unavailable.", typeof(SolrSearchContext));
                return new EnumerableQuery<TItem>(new List<TItem>());
            }
            LinqToSolrIndex<TItem> linqToSolrIndex = new Sitecore.Support.ContentSearch.SolrProvider.LinqToSolrIndex<TItem>(this, executionContexts);
            if (Sitecore.ContentSearch.Utilities.ContentSearchConfigurationSettings.EnableSearchDebug)
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
