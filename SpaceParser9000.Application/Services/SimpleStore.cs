using SpaceParser9000.Core.Interfaces;
using SpaceParser9000.Core.Models;
using System.Text.Json;

namespace SpaceParser9000.Application.Services;

public class SimpleStore : IStore, IDisposable
{
    private readonly Dictionary<string, byte[]> _data = new();
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    private long _setCount = 0;
    private long _getCount = 0;
    private long _deleteCount = 0;

    public void Set(string key, UserProfile profile)
    {
        try
        {
            _lock.EnterWriteLock();

            using MemoryStream ms = new();
            profile.SerializeToBinary(ms);
            var profileBytes = ms.ToArray();
            bool addSuccess = _data.TryAdd(key, profileBytes);
            if (!addSuccess)
                _data[key] = profileBytes;
            Interlocked.Increment(ref _setCount);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public UserProfile? Get(string key)
    {
        bool isSuccess = true;
        try
        {
            _lock.EnterReadLock();
            var bytes = _data.GetValueOrDefault(key);
            var profile = JsonSerializer.Deserialize<UserProfile>(bytes);
            return profile;
        }
        catch
        {
            isSuccess = false;
            throw;
        }
        finally
        {
            if (isSuccess)
                Interlocked.Increment(ref _getCount);
            
            _lock.ExitReadLock();
        }
    }

    public void Delete(string key)
    {
        try
        {
            _lock.EnterWriteLock();
            
            if (_data.Remove(key)) 
                Interlocked.Increment(ref _deleteCount);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public (long setCount, long getCount, long deleteCount) GetStatistics()
    {
        return (_setCount, _getCount, _deleteCount);
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}