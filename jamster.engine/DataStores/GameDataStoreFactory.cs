using System.Collections.Concurrent;

using DotNext.Threading;

namespace jamster.engine.DataStores;

public interface IGameDataStoreFactory
{
    Task<IGameDataStore> GetDataStore(string databaseName);
    Task ReleaseConnection(string databaseName);
    ValueTask<AsyncLock.Holder> AcquireLock();
}

public class GameDataStoreFactory(GameDataStore.Factory gameDataStoreFactory) : IGameDataStoreFactory
{
    private readonly AsyncManualResetEvent _gamesLock = new(false);
    private readonly ConcurrentDictionary<string, AsyncLazy<IGameDataStore>> _dataStores = new();

    public async Task<IGameDataStore> GetDataStore(string databaseName)
    {
        return await _dataStores.GetOrAdd(databaseName, _ => new(async (cancellationToken) =>
        {
            using var @lock = await AcquireLock();
            return gameDataStoreFactory(databaseName);
        })).WithCancellation(default);
    }

    public async Task ReleaseConnection(string databaseName)
    {
        if (!_dataStores.TryRemove(databaseName, out var connection))
            return;
        (await connection.WithCancellation(default)).Dispose();

        GC.Collect();
    }

    // Method used to clean up during integration tests. Ugly and would be nice to find another way asap.
    public async Task ReleaseConnections()
    {
        var stores = await Task.WhenAll(_dataStores.Values.Select(async s => await s.WithCancellation(default)));
        _dataStores.Clear();

        foreach (var store in stores)
        {
            store.Dispose();
        }
    }

    public ValueTask<AsyncLock.Holder> AcquireLock() =>
        _gamesLock.AcquireLockAsync(TimeSpan.FromSeconds(5));

}