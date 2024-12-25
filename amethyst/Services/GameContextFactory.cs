using System.Collections.Concurrent;
using System.Diagnostics;
using amethyst.Extensions;

namespace amethyst.Services;

using System.Collections.Immutable;
using DataStores;
using Reducers;

public interface IGameContextFactory : IDisposable
{
    GameContext GetGame(GameInfo gameInfo);
    void UnloadGame(Guid gameId);
    Task ReloadGame(GameInfo gameInfo);
}

public class GameContextFactory(
    GameStateStoreFactory stateStoreFactory, 
    IEnumerable<ReducerFactory> reducerFactories,
    GameClock.Factory gameClockFactory,
    IGameDataStoreFactory gameStoreFactory,
    ILogger<GameContextFactory> logger) 
    : IGameContextFactory
{
    private readonly ConcurrentDictionary<Guid, Lazy<GameContext>> _gameContexts = [];

    public GameContext GetGame(GameInfo gameInfo) =>
        // GetOrAdd is not thread-safe. The use of Lazy<> ensures that LoadGame only gets called once.
        _gameContexts.GetOrAdd(gameInfo.Id, _ => new(() => LoadGame(gameInfo))).Value;

    public void UnloadGame(Guid gameId)
    {
        if (!_gameContexts.Remove(gameId, out var context))
            return;

        context.Value.Dispose();
    }

    public async Task ReloadGame(GameInfo gameInfo)
    {
        if (!_gameContexts.ContainsKey(gameInfo.Id) || !_gameContexts[gameInfo.Id].IsValueCreated)
            return;

        var context = _gameContexts[gameInfo.Id].Value;
        var stateStore = context.StateStore;
        stateStore.DisableNotifications();
        stateStore.LoadDefaultStates(context.Reducers);

        var game = await gameStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(gameInfo));
        var events = game.GetEvents().ToArray();
        await stateStore.ApplyEvents(context.Reducers, events);

        stateStore.EnableNotifications();
        stateStore.ForceNotify();
    }

    private GameContext LoadGame(GameInfo gameInfo)
    {
        logger.LogInformation("Loading game state for {gameName} ({gameId})", gameInfo.Name, gameInfo.Id);

        var loadTimer = Stopwatch.StartNew();

        var game = gameStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(gameInfo)).Result;

        var events = game.GetEvents().ToArray();

        var stateStore = stateStoreFactory();
        var reducerGameContext = new ReducerGameContext(gameInfo, stateStore);
        var reducers = 
            reducerFactories.Select(f => f(reducerGameContext))
                .SortReducers()
                .ToImmutableList();
        stateStore.LoadDefaultStates(reducers);
        stateStore.ApplyEvents(reducers, events);

        var gameClock = gameClockFactory(gameInfo, reducers.OfType<ITickReceiver>());
        gameClock.Run();

        loadTimer.Stop();
        logger.LogInformation("Loaded game in {loadTime}ms", loadTimer.ElapsedMilliseconds);

        return new GameContext(gameInfo, reducers, stateStore, gameClock);
    }

    public void Dispose()
    {
        foreach (var context in _gameContexts.Values)
        {
            context.Value.Dispose();
        }
        _gameContexts.Clear();
    }
}

public record ReducerGameContext(GameInfo GameInfo, IGameStateStore StateStore);

public record GameContext(
    GameInfo GameInfo,
    IImmutableList<IReducer> Reducers,
    IGameStateStore StateStore,
    IGameClock GameClock) : IDisposable
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        GameClock.Dispose();
    }
}