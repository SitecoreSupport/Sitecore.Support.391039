
namespace Sitecore.Support
{
    public interface IFailResistantIndex
    {
        ConnectionStatus ConnectionStatus { get; set; }

        void Init();
    }
}
