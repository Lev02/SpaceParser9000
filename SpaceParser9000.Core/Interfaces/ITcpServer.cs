namespace SpaceParser9000.Core.Interfaces;

public interface ITcpServer
{
    Task StartAsync(string host, int port, CancellationToken ct);
}