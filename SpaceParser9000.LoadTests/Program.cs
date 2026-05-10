using System.Text.Json;
using NBomber.CSharp;
using SpaceParser9000.Core.Models;
using SpaceParser9000.LoadTests;

static string GenerateRandomString(int length)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    return new string(Enumerable.Range(0, length)
        .Select(_ => chars[Random.Shared.Next(chars.Length)])
        .ToArray());
}

var scenario = Scenario.Create("default_cache_scenario", async context =>
{
    var serverAddress = "127.0.0.1";
    var serverPort = 8080;
    
    var randomKey = GenerateRandomString(10);
    var randomUserProfile = new UserProfile()
    {
        Id = Random.Shared.Next(0, int.MaxValue),
        CreatedAt = new DateTime(Random.Shared.Next(0, int.MaxValue)),
        Username = GenerateRandomString(10),
    };
    
    //в ДЗ сказано создавать клиент на каждом шаге отдельно, но если так делать,
    //то в некоторых сценариях возникает ошибка " Обычно разрешается только одно использование адреса сокета (протокол/сетевой адрес/порт). 127.0.0.1:8080"
    //поэтому я вынес создание сюда
    using var testClient = new TestTcpClient();
    testClient.Connect(serverAddress, serverPort);
    
    var setStep = await Step.Run<object?>("set", context, async() =>
    {
        bool result = await testClient.SetAsync(randomKey, randomUserProfile);
        return result ? Response.Ok() : Response.Fail();
    });
    
    if (setStep.IsError)
        return Response.Fail();

    var getStep1 = await Step.Run<object?>("get1", context, async() =>
    {
        byte[] resultBytes = await testClient.GetAsync(randomKey);
        if (resultBytes.Length <= 0) 
            return Response.Fail();
        
        var userProfile = JsonSerializer.Deserialize<UserProfile>(resultBytes);
        return userProfile == randomUserProfile ? Response.Ok() : Response.Fail();
    });
    
    if (getStep1.IsError)
        return Response.Fail();
    
    var deleteStep = await Step.Run<object?>("delete", context, async() =>
    {
        bool result = await testClient.DeleteAsync(randomKey);
        return result ? Response.Ok() : Response.Fail();
    });
    
    if (deleteStep.IsError)
        return Response.Fail();
    
    var getStep2 = await Step.Run<object?>("get2", context, async() =>
    {
        byte[] resultBytes = await testClient.GetAsync(randomKey);
        return resultBytes is [0] ? Response.Ok() : Response.Fail();
    });

    return getStep2.IsError ? Response.Fail() : Response.Ok();
})
.WithLoadSimulations(
    // Прогрев
    Simulation.RampingInject(rate: 100,
        interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(5)),
    //Стабильная нагрузка
    Simulation.Inject(rate: 100,
        interval: TimeSpan.FromSeconds(1),
        during: TimeSpan.FromSeconds(30))
);

NBomberRunner.RegisterScenarios(scenario).Run();
