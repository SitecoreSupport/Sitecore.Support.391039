
namespace Sitecore.Support.ContentSearch.SolrProvider
{

    public interface IRefreshableSource<T>
    {
        void Refresh(T newSource);
    }
}
