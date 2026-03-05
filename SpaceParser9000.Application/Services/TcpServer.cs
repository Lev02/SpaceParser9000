using System.Buffers;
using System.Collections.Concurrent;
using System.Net.Sockets;
using SpaceParser9000.Application.Extensions;
using SpaceParser9000.Core.Interfaces;

namespace SpaceParser9000.Application.Services;

public class TcpServer : ITcpServer, IDisposable
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _addressInUseDict = new();
    
    public async Task StartAsync(string host, int port, CancellationToken ct = default)
    {
        var address = $"{host}:{port}";

        var semaphore = _addressInUseDict.GetOrAdd(address, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        try
        {
            Console.WriteLine($"TCP-сервер запущен по адресу {address}");

            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(host), port));
            socket.Listen();

            while (!ct.IsCancellationRequested)
            {
                Socket clientSocket = await socket.AcceptAsync(ct);
                _ = HandleClientSocketAsync(clientSocket, ct);
            }
        }
        catch (OperationCanceledException) 
        { }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task HandleClientSocketAsync(Socket clientSocket, CancellationToken ct = default)
    {
        try
        {
            while (true)
            {
                var arrayPool = ArrayPool<byte>.Shared.Rent(1024);
                try
                {
                    int bytesReadCount = await clientSocket.ReceiveAsync(arrayPool, ct);

                    if (bytesReadCount == 0)
                        return;

                    ReadOnlyMemory<byte> bytesRead = arrayPool.AsMemory(0, bytesReadCount);
                    
                    var charArray = ArrayPool<char>.Shared.Rent(bytesReadCount);
                    try
                    {
                        int charCount = System.Text.Encoding.UTF8.GetChars(bytesRead.Span, charArray);
                        CommandParser.Parse(charArray.AsSpan(0, charCount)).SendToCommandLine();
                    }
                    finally
                    {
                        ArrayPool<char>.Shared.Return(charArray);
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(arrayPool);
                }
            }
        }
        finally
        {
            clientSocket.Dispose();
        }
    }

    public void Dispose()
    {
        foreach (var (address, semaphore) in _addressInUseDict)
        {
            if (_addressInUseDict.TryRemove(address, out _))
            {
                semaphore.Dispose();
            }
        }
    }
}