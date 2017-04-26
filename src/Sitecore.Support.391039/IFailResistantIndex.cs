
namespace Sitecore.Support
{
    public interface IFailResistantIndex
    {
        string Name { get; }

        ConnectionStatus ConnectionStatus { get; }

        void SetStatus(ConnectionStatus status);

        void Connect();

        ConnectionStatus CheckStatus();
    }
}
