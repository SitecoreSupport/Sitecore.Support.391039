namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Linq;
    using Sitecore.ContentSearch.Linq.Common;
    using Sitecore.ContentSearch.Linq.Solr;
    using Sitecore.ContentSearch.Utilities;
    using Sitecore.Diagnostics;
    using SolrNet;

    public class LinqToSolrIndex<TItem> : Sitecore.ContentSearch.SolrProvider.LinqToSolrIndex<TItem>
    {
        private static readonly MethodInfo miApplyScalarMethods;

        static LinqToSolrIndex()
        {
            miApplyScalarMethods = typeof(Sitecore.ContentSearch.SolrProvider.LinqToSolrIndex<TItem>)
                .GetMethod("ApplyScalarMethods", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(miApplyScalarMethods, "Can't find ApplyScalarMethods method");
        }

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

            return this.CreateEmptyObject<TResult>(compositeQuery);
        }

        protected virtual bool IsIndexAvailable(ISearchIndex index)
        {
            var failResistantIndex = index as IFailResistantIndex;

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

        protected virtual TResult CreateEmptyObject<TResult>(SolrCompositeQuery compositeQuery)
        {
            Type documentType;

            if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(SearchResults<>))
            {
                documentType = typeof(TResult).GetGenericArguments()[0];
            }
            else
            {
                documentType = typeof(TResult);
            }

            Assembly assembly = Assembly.GetAssembly(typeof(Sitecore.ContentSearch.SolrProvider.SolrSearchIndex));
            Type solrSearchResultsType = assembly.GetType("Sitecore.ContentSearch.SolrProvider.SolrSearchResults`1", true);

            var solrSearchResultsGenericType = solrSearchResultsType.MakeGenericType(documentType);

            var applyScalarMethodsMethod = typeof(Sitecore.ContentSearch.SolrProvider.LinqToSolrIndex<TItem>)
                .GetMethod("ApplyScalarMethods", BindingFlags.Instance | BindingFlags.NonPublic);
            var applyScalarMethodsGenericMethod = applyScalarMethodsMethod.MakeGenericMethod(typeof(TResult),
                documentType);

            var emptyResults = new SolrQueryResults<Dictionary<string, object>>();

            var processedResults = ReflectionUtility.CreateInstance(solrSearchResultsGenericType, this.context, emptyResults,
                null, compositeQuery.ExecutionContexts, compositeQuery.VirtualFieldProcessors);

            var resultObject = applyScalarMethodsGenericMethod.Invoke(this, new object[] { compositeQuery, processedResults, emptyResults });

            return (TResult) resultObject;
        }

        [UsedImplicitly]
        private TResult ApplyScalarMethods<TResult, TDocument>(SolrCompositeQuery compositeQuery, object processedResults, SolrQueryResults<Dictionary<string, object>> results)
        {
            var documentType = typeof(TResult).GetGenericArguments()[0];
            var miApplyScalarMethodsGeneric = miApplyScalarMethods.MakeGenericMethod(typeof(TResult), documentType);
            return (TResult)miApplyScalarMethodsGeneric.Invoke(this, new object[] { compositeQuery, processedResults, results });
        }
    }
}
