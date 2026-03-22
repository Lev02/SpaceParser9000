using SpaceParser9000.Application.Services;

var tcpServer = new TcpServer(new SimpleStore());
var cts = new CancellationTokenSource();
_ = tcpServer.StartAsync("127.0.0.1", 8080, cts.Token);

Console.WriteLine("Нажмите любую клавишу, чтобы завершить работу сервера");
Console.ReadKey();

await cts.CancelAsync();
