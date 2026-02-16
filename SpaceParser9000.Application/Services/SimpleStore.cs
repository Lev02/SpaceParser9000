using SpaceParser9000.Core.Interfaces;

namespace SpaceParser9000.Application.Services;

public class SimpleStore : IStore, IDisposable
{
    private readonly Dictionary<string, byte[]> _data = new();
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    private long _setCount = 0;
    private long _getCount = 0;
    private long _deleteCount = 0;
    
    public void Set(string key, byte[] value)
    {
        try
        {
            _lock.EnterWriteLock();
            
            bool addSuccess = _data.TryAdd(key, value);
            if (!addSuccess)
                _data[key] = value;
            Interlocked.Increment(ref _setCount);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public byte[]? Get(string key)
    {
        bool isSuccess = true;
        try
        {
            _lock.EnterReadLock();
            return _data.GetValueOrDefault(key);
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