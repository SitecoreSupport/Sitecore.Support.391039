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
    using Sitecore.ContentSearch.Utilities;
    using Sitecore.Diagnostics;
    using SolrNet;

    public class LinqToSolrIndex<TItem> : Sitecore.ContentSearch.SolrProvider.LinqToSolrIndex<TItem>
    {
        private readonly Sitecore.Support.ContentSearch.SolrProvider.SolrSearchContext context;

        private static readonly MethodInfo executeMethodInfo;

        static LinqToSolrIndex()
        {
            executeMethodInfo = typeof(Sitecore.ContentSearch.SolrProvider.LinqToSolrIndex<TItem>)
                .GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(executeMethodInfo, "Could not find 'Execute' method in type Sitecore.ContentSearch.SolrProvider.LinqToSolrIndex<TItem>");
        }

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
            var failResistantSolrSearchIndex = this.context.Index as Sitecore.Support.ContentSearch.SolrProvider.SolrSearchIndex;

            if (failResistantSolrSearchIndex != null)
            {
                if (failResistantSolrSearchIndex.PreviousConnectionStatus == ConnectionStatus.Succeded)
                {
                    return base.FindElements<TElement>(compositeQuery);
                }
                else
                {
                    Log.Error("SUPPORT: unable to execute a search query. Solr core [" + failResistantSolrSearchIndex.Core + "] is unavailable.", typeof(SolrSearchContext));
                }
            }

            var failResistantSwitchOnRebuildSolrSearchIndex = this.context.Index as Sitecore.Support.ContentSearch.SolrProvider.SwitchOnRebuildSolrSearchIndex;
            if (failResistantSwitchOnRebuildSolrSearchIndex != null)
            {
                if (failResistantSwitchOnRebuildSolrSearchIndex.PreviousConnectionStatus == ConnectionStatus.Succeded)
                {
                    return base.FindElements<TElement>(compositeQuery);
                }

                Log.Error("SUPPORT: unable to execute a search query. Solr core [" + failResistantSwitchOnRebuildSolrSearchIndex.Core + "] is unavailable.", this);
            }

            return new System.Collections.Generic.List<TElement>();
        }

        public override TResult Execute<TResult>(SolrCompositeQuery compositeQuery)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Sitecore.ContentSearch.SolrProvider.SolrSearchIndex));
            Type solrSearchResultsType = assembly.GetType("Sitecore.ContentSearch.SolrProvider.SolrSearchResults`1", true);
            if (typeof(TResult).IsGenericType && (typeof(TResult).GetGenericTypeDefinition() == typeof(SearchResults<>)))
            {
                System.Type resultType = typeof(TResult).GetGenericArguments()[0];

                SolrQueryResults<System.Collections.Generic.Dictionary<string, object>> results = null;
                var failResistantSolrSearchIndex = this.context.Index as Sitecore.Support.ContentSearch.SolrProvider.SolrSearchIndex;
                if (failResistantSolrSearchIndex != null)
                {
                    if (failResistantSolrSearchIndex.PreviousConnectionStatus != ConnectionStatus.Succeded)
                    {
                        results = new SolrQueryResults<Dictionary<string, object>>();
                        Log.Error("SUPPORT: unable to execute a search query. Solr core [" + failResistantSolrSearchIndex.Core + "] is unavailable.", typeof(SolrSearchContext));
                    }
                }
                var failResistantSwitchOnRebuildSolrSearchIndex = this.context.Index as Sitecore.Support.ContentSearch.SolrProvider.SwitchOnRebuildSolrSearchIndex;
                if (failResistantSwitchOnRebuildSolrSearchIndex != null)
                {
                    if (failResistantSwitchOnRebuildSolrSearchIndex.PreviousConnectionStatus != ConnectionStatus.Succeded)
                    {
                        results = new SolrQueryResults<Dictionary<string, object>>();
                        Log.Error("SUPPORT: unable to execute a search query. Solr core [" + failResistantSwitchOnRebuildSolrSearchIndex.Core + "] is unavailable.", typeof(SolrSearchContext));
                    }
                }
                if (results == null)
                { 
                    results = (SolrQueryResults<System.Collections.Generic.Dictionary<string, object>>)executeMethodInfo.Invoke(this, new object[] { compositeQuery, typeof(TResult) });
                }
                System.Type type3 = solrSearchResultsType.MakeGenericType(new System.Type[] { resultType });
                System.Reflection.MethodInfo info2 = base.GetType().BaseType.GetMethod("ApplyScalarMethods", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).MakeGenericMethod(new System.Type[] { typeof(TResult), resultType });
                SelectMethod method = LinqToSolrIndex<TItem>.GetSelectMethod(compositeQuery);
                object obj2 = ReflectionUtility.CreateInstance(type3, (object[])new object[] { this.context, results, method, compositeQuery.ExecutionContexts, compositeQuery.VirtualFieldProcessors });
                return (TResult)info2.Invoke(this, (object[])new object[] { compositeQuery, obj2, results });
            }
            SolrQueryResults<System.Collections.Generic.Dictionary<string, object>> searchResults = null;
            var failResistantSolrSearchIndex2 = this.context.Index as Sitecore.Support.ContentSearch.SolrProvider.SolrSearchIndex;
            if (failResistantSolrSearchIndex2 != null)
            {
                if (failResistantSolrSearchIndex2.PreviousConnectionStatus != ConnectionStatus.Succeded)
                {
                    searchResults = new SolrQueryResults<Dictionary<string, object>>();
                    Log.Error("SUPPORT: unable to execute a search query. Solr core [" + failResistantSolrSearchIndex2.Core + "] is unavailable.", typeof(SolrSearchContext));
                }
            }
            var failResistantSwitchOnRebuildSolrSearchIndex2 = this.context.Index as Sitecore.Support.ContentSearch.SolrProvider.SwitchOnRebuildSolrSearchIndex;
            if (failResistantSwitchOnRebuildSolrSearchIndex2 != null)
            {
                if (failResistantSwitchOnRebuildSolrSearchIndex2.PreviousConnectionStatus != ConnectionStatus.Succeded)
                {
                    searchResults = new SolrQueryResults<Dictionary<string, object>>();
                    Log.Error("SUPPORT: unable to execute a search query. Solr core [" + failResistantSwitchOnRebuildSolrSearchIndex2.Core + "] is unavailable.", typeof(SolrSearchContext));
                }
            }

            if (searchResults == null)
            { 
                searchResults =
                    (SolrQueryResults<System.Collections.Generic.Dictionary<string, object>>)
                        executeMethodInfo.Invoke(this, new object[] { compositeQuery, typeof(TResult) });
            }
            SelectMethod selectMethod = LinqToSolrIndex<TItem>.GetSelectMethod(compositeQuery);
            var processedResults = (solrSearchResultsType.MakeGenericType(typeof(TResult)).GetConstructor(new Type[] { typeof(SolrSearchContext), typeof(SolrQueryResults<Dictionary<string, object>>), typeof(SelectMethod), typeof(IEnumerable<IExecutionContext>), typeof(IEnumerable<IFieldQueryTranslator>) })).Invoke(new object[] { this.context, searchResults, selectMethod, compositeQuery.ExecutionContexts, compositeQuery.VirtualFieldProcessors });
            var ApplyScalarMethodsMethodInfo = base.GetType().BaseType.GetMethod("ApplyScalarMethods", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).MakeGenericMethod(new System.Type[] { typeof(TResult), typeof(TResult) });
            return (TResult)ApplyScalarMethodsMethodInfo.Invoke(this, new object[] { compositeQuery, processedResults, searchResults });
        }

        /// <summary>Gets the select method.</summary>
        /// <param name="compositeQuery">The composite query.</param>
        /// <returns>Select method</returns>
        private static SelectMethod GetSelectMethod(SolrCompositeQuery compositeQuery)
        {
            var selectMethods = compositeQuery.Methods.Where(m => m.MethodType == QueryMethodType.Select).Select(m => (SelectMethod)m).ToList();

            return selectMethods.Count() == 1 ? selectMethods[0] : null;
        }
    }
}
