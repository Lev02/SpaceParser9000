using BenchmarkDotNet.Attributes;
using SpaceParser9000.Core.Models;

namespace SpaceParser9000.LoadTests;

[MemoryDiagnoser]
public class Benchmarks
{
    private UserProfile _profile = null!;

    [GlobalSetup]
    public void Setup()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var username = new string(Enumerable.Range(0, 10)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());

        _profile = new UserProfile
        {
            CreatedAt = DateTime.Now,
            Id = 1,
            Username = username
        };
    }

    [Benchmark(Baseline = true)]
    public byte[] SerializeWithSystemTextJson()
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(_profile);
    }

    [Benchmark]
    public byte[] SerializeWithBinaryWriter()
    {
        using var ms = new MemoryStream();
        _profile.SerializeToBinary(ms);
        return ms.ToArray();
    }
}