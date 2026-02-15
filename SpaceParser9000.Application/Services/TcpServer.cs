using System.Buffers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using SpaceParser9000.Application.Extensions;
using SpaceParser9000.Core.Interfaces;
using SpaceParser9000.Core.Models;

namespace SpaceParser9000.Application.Services;

public class TcpServer : ITcpServer
{
    public async Task StartAsync(string host, int port, CancellationToken ct = default)
    {
        Console.WriteLine($"TCP-сервер запущен по адресу {host}:{port}");

        try
        {
            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ipAddress = System.Net.IPAddress.Parse(host);
            var endPoint = new System.Net.IPEndPoint(ipAddress, port);
            socket.Bind(endPoint);
            socket.Listen();

            while (true)
            {
                Socket clientSocket = await socket.AcceptAsync(ct);
                _ = HandleClientSocketAsync(clientSocket, ct);
            }
        }
        catch (OperationCanceledException)
        {
            
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
}