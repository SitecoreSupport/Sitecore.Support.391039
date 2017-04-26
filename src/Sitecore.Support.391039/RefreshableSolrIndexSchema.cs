
namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using System.Collections.Generic;
    using System.Linq;
    using Diagnostics;
    using SolrNet.Schema;
    using Sitecore.ContentSearch.SolrProvider;

    public class RefreshableSolrIndexSchema : SolrIndexSchema, IRefreshableSource<SolrSchema>
    {
        protected List<string> allCultures;
        protected List<string> allFields;
        protected SolrSchema schema;

        protected readonly object locker = new object();

        public RefreshableSolrIndexSchema(SolrSchema schema) : base(schema)
        {
            this.DoInit(schema);
        }

        protected void DoInit(SolrSchema newSchema)
        {
            Assert.ArgumentNotNull(newSchema, "newSchema");

            var newAllFields = newSchema.SolrFields.Select(x => x.Name).ToList();
            var newAllCultures = newSchema.SolrDynamicFields
                .Where(x => x.Name.StartsWith("*_t_")).Select(x => x.Name.Replace("*_t", string.Empty)).ToList();

            lock (this.locker)
            { 
                this.schema = newSchema;
                this.allFields = newAllFields;
                this.allCultures = newAllCultures;
            }
        }

        public virtual void Refresh(SolrSchema newSchema)
        {
            this.DoInit(newSchema);
        }

        public override ICollection<string> AllCultures
        {
            get
            {
                lock (this.locker)
                {
                    return this.allCultures;
                }
            }
        }

        public override ICollection<string> AllFieldNames
        {
            get
            {
                lock (this.locker)
                {
                    return this.allFields;
                }
            }
        }
    }
}
