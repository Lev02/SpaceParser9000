using System.Buffers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using SpaceParser9000.Core.Models;

namespace SpaceParser9000.LoadTests;

public class TestTcpClient : IDisposable
{
    private readonly Socket _clientSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public void Connect(string host, int port)
    {
        var address = $"{host}:{port}";

        Console.WriteLine($"TCP-клиент запущен по адресу {address}");
        _clientSocket.Connect(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(host), port));
    }
    
    public async Task<bool> SetAsync(string key, UserProfile profile)
    {
        var profileJson = JsonSerializer.Serialize(profile);
        var requestBytes = Encoding.UTF8.GetBytes($"SET {key} {profileJson}");
        await _clientSocket.SendAsync(requestBytes, SocketFlags.None);
        
        var result = await GetServerSocketResponseAsync();
        return result.FirstOrDefault() == 1;
    }

    public async Task<byte[]> GetAsync(string key)
    {
        var requestBytes = Encoding.UTF8.GetBytes($"GET {key}");
        await _clientSocket.SendAsync(requestBytes);
        
        var result = await GetServerSocketResponseAsync();
        return result;
    }
    
    public async Task<bool> DeleteAsync(string key)
    {
        var requestBytes = Encoding.UTF8.GetBytes($"DELETE {key}");
        await _clientSocket.SendAsync(requestBytes);
        
        var result = await GetServerSocketResponseAsync();
        return result.FirstOrDefault() == 1;
    }

    /// <returns>true if server response was OK, otherwise false</returns>
    private async Task<byte[]> GetServerSocketResponseAsync()
    {
        var arrayPool = ArrayPool<byte>.Shared.Rent(1024);
        try
        {
            int bytesReadCount = await _clientSocket.ReceiveAsync(arrayPool);

            if (bytesReadCount == 0)
                return [0];
            
            return arrayPool.Take(bytesReadCount).ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(arrayPool);
        }
    }

    public void Dispose()
    {
        _clientSocket.Dispose();
    }
}