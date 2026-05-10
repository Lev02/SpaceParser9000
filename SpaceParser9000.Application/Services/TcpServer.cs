using System.Buffers;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using SpaceParser9000.Application.Extensions;
using SpaceParser9000.Core.Interfaces;
using SpaceParser9000.Core.Models;

namespace SpaceParser9000.Application.Services;

public class TcpServer : ITcpServer, IDisposable
{
    private const string NotFoundMessage = "NOT FOUND";
    private const string UnknownCommandMessage = "UNKNOWN COMMAND";
    private const string CouldNotParseMessage = "COULD NOT PARSE USER PROFILE JSON";
    private readonly IStore _store;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _addressInUseDict = new();

    public TcpServer(IStore store)
    {
        _store = store;
    }
    
    public async Task StartAsync(string host, int port, CancellationToken ct = default)
    {
        var address = $"{host}:{port}";

        var semaphore = _addressInUseDict.GetOrAdd(address, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        try
        {
            Console.WriteLine($"TCP-сервер запущен по адресу {address}");

            using Socket serverSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Parse(host), port));
            serverSocket.Listen();

            while (!ct.IsCancellationRequested)
            {
                Socket clientSocket = await serverSocket.AcceptAsync(ct);
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
                        var result = ExecuteClientRequest(charArray, bytesRead);
                        await clientSocket.SendAsync(result, ct);
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

    /// <returns>true if request was parsed successfully, otherwise false</returns>
    private byte[] ExecuteClientRequest(char[] charArray, ReadOnlyMemory<byte> bytesRead)
    {
        int charCount = System.Text.Encoding.UTF8.GetChars(bytesRead.Span, charArray);
        var command = CommandParser.Parse(charArray.AsSpan(0, charCount));
        
        switch (command.CommandName)
        {
            case "GET":
                command.SendToCommandLine();
                var getProfile = _store.Get(command.Key.ToString());
                var getProfileBytes = JsonSerializer.SerializeToUtf8Bytes(getProfile);
                Console.WriteLine(getProfile == null
                    ? $"GET RESULT: {NotFoundMessage}"
                    : $"GET RESULT: {Encoding.UTF8.GetString(getProfileBytes)}");
                return getProfile == null ? [0] : getProfileBytes;
            case "SET":
                command.SendToCommandLine();
                var setProfile = JsonSerializer.Deserialize<UserProfile>(command.Value);
                if (setProfile == null)
                {
                    Console.WriteLine($"{CouldNotParseMessage}: {command.Value}");
                    return [0];
                }
                _store.Set(command.Key.ToString(), setProfile);
                return [1];
            case "DELETE":
                command.SendToCommandLine();
                _store.Delete(command.Key.ToString());
                return [1];
            default:
                Console.WriteLine($"{UnknownCommandMessage}: {charArray.AsSpan(0, charCount).ToString()}");
                return [0];
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