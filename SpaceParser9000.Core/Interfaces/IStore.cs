namespace SpaceParser9000.Core.Interfaces;

public interface IStore
{
    void Set(string key, byte[] value);
    byte[]? Get(string key);
    void Delete(string key);
}