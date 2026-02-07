using SpaceParser9000.Core.Interfaces;

namespace SpaceParser9000.Application.Services;

public class SimpleStore : IStore
{
    private readonly Dictionary<string, byte[]> _data = new();
    
    public void Set(string key, byte[] value)
    {
        bool addSuccess = _data.TryAdd(key, value);
        if (!addSuccess)
            _data[key] = value;
    }

    public byte[]? Get(string key)
    {
        return _data.GetValueOrDefault(key);
    }

    public void Delete(string key)
    {
        _data.Remove(key);
    }
}