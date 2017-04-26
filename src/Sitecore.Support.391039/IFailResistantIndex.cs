
namespace Sitecore.Support
{
    public interface IFailResistantIndex
    {
        ConnectionStatus ConnectionStatus { get; }

        void SetStatus(ConnectionStatus status);

        void Connect();

        ConnectionStatus RefreshStatus();
    }
}
