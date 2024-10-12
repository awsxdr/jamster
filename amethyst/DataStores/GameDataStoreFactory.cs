using System.Collections.Concurrent;

namespace amethyst.DataStores;

public interface IGameDataStoreFactory
{
    IGameDataStore GetDataStore(string databaseName);
}

public class GameDataStoreFactory(GameDataStore.Factory gameDataStoreFactory) : IGameDataStoreFactory
{
    private readonly ConcurrentDictionary<string, Lazy<IGameDataStore>> _dataStores = new();

    public IGameDataStore GetDataStore(string databaseName) =>
        _dataStores.GetOrAdd(databaseName, _ => new(() => gameDataStoreFactory(databaseName))).Value;

    // Method used to clean up during integration tests. Ugly and would be nice to find another way asap.
    public void ReleaseConnections()
    {
        var stores = _dataStores.Values.Select(s => s.Value);
        _dataStores.Clear();

        foreach (var store in stores)
        {
            store.Dispose();
        }
    }
}