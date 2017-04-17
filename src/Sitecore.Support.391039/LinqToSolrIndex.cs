namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Sitecore.ContentSearch.Linq;
    using Sitecore.ContentSearch.Linq.Common;
    using Sitecore.ContentSearch.Linq.Methods;
    using Sitecore.ContentSearch.Linq.Nodes;
    using Sitecore.ContentSearch.Linq.Solr;
  
    using Sitecore.Diagnostics;
    using SolrNet;

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
            var failResistantIndex = this.context.Index as IFailResistantIndex;

            if (failResistantIndex != null)
            {
                if (failResistantIndex.ConnectionStatus != ConnectionStatus.Succeded)
                {
                    var solrIndex = this.context.Index as Sitecore.ContentSearch.SolrProvider.SolrSearchIndex;
                    Log.Error("SUPPORT: unable to execute a search query. Solr core [" + solrIndex.Core + "] is unavailable.", this);
                    return new System.Collections.Generic.List<TElement>();
                }
            }

            return base.FindElements<TElement>(compositeQuery);
        }

        public override TResult Execute<TResult>(SolrCompositeQuery compositeQuery)
        {
            var failResistantIndex = this.context.Index as IFailResistantIndex;

            if (failResistantIndex != null)
            {
                if (failResistantIndex.ConnectionStatus != ConnectionStatus.Succeded)
                {
                    var solrIndex = this.context.Index as Sitecore.ContentSearch.SolrProvider.SolrSearchIndex;
                    Log.Error("SUPPORT: unable to execute a search query. Solr core [" + solrIndex.Core + "] is unavailable.", this);
                    return default(TResult);
                }
            }

            return base.Execute<TResult>(compositeQuery);
        }
    }
}
