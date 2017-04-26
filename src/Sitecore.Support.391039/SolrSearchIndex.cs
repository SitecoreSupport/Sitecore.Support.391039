
namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Diagnostics;
    using Microsoft.Practices.ServiceLocation;
    using Sitecore.ContentSearch.Security;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Maintenance;
    using Sitecore.ContentSearch.SolrProvider;
    using SolrNet;
    using SolrNet.Exceptions;
    using SolrNet.Impl;
    using SolrNet.Schema;
    using Utils;

    public class SolrSearchIndex : Sitecore.ContentSearch.SolrProvider.SolrSearchIndex, IFailResistantIndex
    {
        protected ConnectionStatus curConnectionStatus = ConnectionStatus.Unknown;

        protected static readonly Type solrSearchIndexType = typeof(Sitecore.ContentSearch.SolrProvider.SolrSearchIndex);

        protected IRefreshableSource<SolrSchema> refreshableSolrIndexSchema;

        protected IRefreshableSource<CoreResult> refreshableCoreResult;

        protected ISolrOperations<Dictionary<string, object>> SolrOperations { get; private set; }

        public SolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore) : this(name, core, propertyStore, null)
        {
        }

        public SolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore, string group) : base(name, core, propertyStore, group)
        {
        }

        public override IProviderSearchContext CreateSearchContext(SearchSecurityOptions options = SearchSecurityOptions.Default)
        {
            return new Sitecore.Support.ContentSearch.SolrProvider.SolrSearchContext(this, options);
        }

        
        public override void Initialize()
        {
            this.DoInitialize();

            if (this.ConnectionStatus == ConnectionStatus.Unknown)
            {
                if (this.CheckStatus() == ConnectionStatus.Succeded)
                {
                    this.Connect();
                }
            }
        }


        protected virtual void DoInitialize()
        {
            if (this.PropertyStore == null)
            {
                throw new ConfigurationErrorsException("Index PropertyStore have not been configured.");
            }

            //this.solrOperations = ServiceLocator.Current.GetInstance<ISolrOperations<Dictionary<string, object>>>(this.Core);
            var solrOperations = ServiceLocator.Current.GetInstance<ISolrOperations<Dictionary<string, object>>>(this.Core);
            ReflectionHelper.SetValueToPrivateField(solrSearchIndexType, this, "solrOperations", solrOperations);
            this.SolrOperations = solrOperations;

            try
            {
                // Previous: var status = solrAdmin.Status(this.Core).Single(); 
                var status = new RefreshableCoreResult(this.Core, true);
                var summary = new SolrIndexSummary(status, this);

                this.refreshableCoreResult = status;
                // this.summary = new SolrIndexSummary(status, this); 
                ReflectionHelper.SetValueToPrivateField(solrSearchIndexType, this, "summary", summary);

                // this.schema = new SolrIndexSchema(this.SolrOperations.GetSchema());
                // Previous: var schema = new SolrIndexSchema(solrOperations.GetSchema()); 
                var schema = new RefreshableSolrIndexSchema(new SolrSchema());
                this.refreshableSolrIndexSchema = schema;
                ReflectionHelper.SetValueToPrivateField(solrSearchIndexType, this, "schema", schema);
            }
            catch (Exception exception)
            {
                this.ProcessSolrConnectionException(exception);

                throw;
            }


            var strategies = ReflectionHelper.GetValueOfPrivateField(solrSearchIndexType, this, "strategies");
            InitializeSearchIndexInitializables(this.Configuration,
                this.Crawlers,
                strategies,
                this.ShardingStrategy);

            var config = this.Configuration as SolrIndexConfiguration;

            if (config == null)
            {
                return;
            }

            // this.fieldNameTranslator = new SolrFieldNameTranslator(this);
            var fieldNameTranslator = new SolrFieldNameTranslator(this);
            ReflectionHelper.SetValueToPrivateField(solrSearchIndexType, this, "fieldNameTranslator", fieldNameTranslator);

            if (this.CommitPolicyExecutor == null)
            {
                this.CommitPolicyExecutor = new NullCommitPolicyExecutor();
            }

            // this.isSharded = this.ShardingStrategy != null;
            var isSharded = this.ShardingStrategy != null;
            ReflectionHelper.SetValueToPrivateField(solrSearchIndexType, this, "isSharded", isSharded);

            this.initialized = true;
        }


        public virtual ConnectionStatus ConnectionStatus
        {
            get
            {
                return this.curConnectionStatus;
            }
        }

        public virtual void SetStatus(ConnectionStatus status)
        {
            this.curConnectionStatus = status;
        }

        public virtual void Connect()
        {
            // this.solrAdmin = SolrContentSearchManager.SolrAdmin;

            var solrAdmin = ReflectionHelper.GetValueOfPrivateField(solrSearchIndexType, this, "solrAdmin") as ISolrCoreAdmin;

            if (solrAdmin == null)
            {
                solrAdmin = SolrContentSearchManager.SolrAdmin;
                if (solrAdmin == null)
                {
                    Log.Warn("SUPPORT: Can't find SolrAdmin", this);
                    return;
                }

                ReflectionHelper.SetValueToPrivateField(solrSearchIndexType, this, "solrAdmin", solrAdmin);
            }

            try
            {
                var status = solrAdmin.Status(this.Core).Single();
                this.refreshableCoreResult.Refresh(status);

                var schema = this.SolrOperations.GetSchema();
                this.refreshableSolrIndexSchema.Refresh(schema);

                this.curConnectionStatus = ConnectionStatus.Succeded;
            }
            catch (Exception exception)
            {
                this.ProcessSolrConnectionException(exception);

                this.curConnectionStatus = ConnectionStatus.Failed;

                throw;
            }
        }

        protected virtual void ProcessSolrConnectionException(Exception exception)
        {

            if (exception is SolrConnectionException)
            {
                if (exception.Message.Contains("java.lang.IllegalStateException") &&
                    exception.Message.Contains("appears both in delegate and in cache"))
                {
                    Log.Warn(
                        $"SUPPORT: Status check for [{this.Core}] Solr core failed. Error suppressed as not related to Solr core availability. Details: https://issues.apache.org/jira/browse/LUCENE-7188",
                        this);
                    return;
                }

            }

            Log.Error($"SUPPORT: Status check for [{this.Core}] Solr core failed.", exception, this);
        }

        public virtual ConnectionStatus CheckStatus()
        {
            ISolrCoreAdmin solrAdmin = SolrContentSearchManager.SolrAdmin;

            if (solrAdmin == null)
            {
                return ConnectionStatus.Unknown;
            }

            try
            {
                var newStatus = solrAdmin.Status(this.Core).FirstOrDefault();

                // The response must contain index name otherwise the core doesn't exist
                return newStatus.Index == null ? ConnectionStatus.Failed : ConnectionStatus.Succeded;
            }
            catch (SolrConnectionException ex)
            {
                if (ex.Message.Contains("java.lang.IllegalStateException") &&
                    ex.Message.Contains("appears both in delegate and in cache"))
                {
                    Log.Warn(
                        $"SUPPORT: Status check for [{this.Core}] Solr core failed. Error suppressed as not related to Solr core availability. Details: https://issues.apache.org/jira/browse/LUCENE-7188",
                        this);
                }
                else
                {
                    Log.Warn($"SUPPORT: Status check for [{this.Core}] Solr core failed.", ex, this);
                }

                return ConnectionStatus.Failed;
            }
        }
    }
}

