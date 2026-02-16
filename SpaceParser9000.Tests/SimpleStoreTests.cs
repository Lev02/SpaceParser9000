using SpaceParser9000.Application.Services;
using Xunit;

namespace SpaceParser9000.Tests;

public class SimpleStoreTests
{
    [Fact]
    public async Task SimpleStoreMethods_WhenInvokedFromMultipleThreads_SetsCountersCorrectly()
    {
        //Arrange
        var store = new SimpleStore();
        int expectedSetCount = 1000;
        int expectedGetCount = 1000;
        int expectedDeleteCount = 0;
        string mainKey = "mainKey";
        Random rnd = new Random();
        
        var setTasks = new List<Task>();
        var getTasks = new List<Task>();
        
        //Act
        for (int i = 1; i <= expectedSetCount; i++)
        {
            setTasks.Add(Task.Run(() => store.Set(mainKey, [ (byte)rnd.Next(1, 255) ])));
        }
        
        for (int i = 0; i < expectedGetCount; i++)
        {
            getTasks.Add(Task.Run(() =>
            {
                store.Get(mainKey);
            }));
        }
        
        await Task.WhenAll(getTasks.Concat(setTasks));
        var (actualSetCount, actualGetCount, actualDeleteCount) = store.GetStatistics();
        
        //Assert
        Assert.Equal(actualSetCount, expectedSetCount);
        Assert.Equal(actualGetCount, expectedGetCount);
        Assert.Equal(actualDeleteCount, expectedDeleteCount);
    }
}