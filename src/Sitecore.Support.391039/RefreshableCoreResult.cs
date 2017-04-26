
namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using Diagnostics;
    using SolrNet.Impl;

    public class RefreshableCoreResult : CoreResult, IRefreshableSource<CoreResult>
    {
        public RefreshableCoreResult()
        {
        }

        public RefreshableCoreResult(string coreName) : base(coreName)
        {
        }

        public RefreshableCoreResult(string coreName, bool prefill) : base(coreName)
        {
            if (prefill)
            {
                this.Index = new CoreIndexResult();
            }
        }

        public virtual void Refresh(CoreResult newCoreResult)
        {
            Assert.ArgumentNotNull(newCoreResult, "newCoreResult");

            this.Name = newCoreResult.Name;
            this.DataDir = newCoreResult.DataDir;
            this.Index = newCoreResult.Index;
            this.InstanceDir = newCoreResult.InstanceDir;
            this.StartTime = newCoreResult.StartTime;
            this.Uptime = newCoreResult.Uptime;
        }
    }
}
