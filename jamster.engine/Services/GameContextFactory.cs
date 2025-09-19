using System.Collections.Concurrent;
using System.Diagnostics;
using System.Collections.Immutable;
using jamster.DataStores;
using jamster.Domain;
using jamster.Reducers;

namespace jamster.Services;

public interface IGameContextFactory : IDisposable
{
    GameContext GetGame(GameInfo gameInfo);
    void UnloadGame(Guid gameId);
    Task ReloadGame(GameInfo gameInfo);
    Task ApplyKeyFrame(GameInfo gameIndo, KeyFrame keyFrame);
}

[Singleton]
public class GameContextFactory(
    GameStateStoreFactory stateStoreFactory, 
    IEnumerable<ReducerFactory> reducerFactories,
    GameClock.Factory gameClockFactory,
    IGameDataStoreFactory gameStoreFactory,
    IKeyFrameService.Factory keyFrameServiceFactory,
    KeyFrameSettings keyFrameSettings,
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

        var (_, reducers, stateStore, gameClock, _) = _gameContexts[gameInfo.Id].Value;
        stateStore.DisableNotifications();
        gameClock.Stop();
        stateStore.LoadDefaultStates(reducers);

        var game = await gameStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(gameInfo));
        var events = game.GetEvents().ToArray();
        await stateStore.ApplyEvents(reducers, null, events);

        stateStore.EnableNotifications();
        gameClock.Run();
        stateStore.ForceNotify();
    }

    public async Task ApplyKeyFrame(GameInfo gameInfo, KeyFrame keyFrame)
    {
        if (!_gameContexts.ContainsKey(gameInfo.Id) || !_gameContexts[gameInfo.Id].IsValueCreated)
            return;

        var (_, reducers, stateStore, gameClock, keyFrameService) = _gameContexts[gameInfo.Id].Value;
        stateStore.DisableNotifications();
        gameClock.Stop();
        stateStore.LoadDefaultStates(reducers);

        stateStore.ApplyKeyFrame(reducers, keyFrame);
        keyFrameService.ClearFramesAfter(keyFrame.Tick);

        var gameDataStore = await gameStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(gameInfo));
        var subsequentEvents = gameDataStore.GetEvents().Where(e => e.Id.Tick > keyFrame.Tick).ToArray();

        await stateStore.ApplyEvents(reducers, null, subsequentEvents);

        stateStore.EnableNotifications();
        gameClock.Run();
        stateStore.ForceNotify();
    }

    private GameContext LoadGame(GameInfo gameInfo)
    {
        logger.LogInformation("Loading game state for {gameName} ({gameId})", gameInfo.Name, gameInfo.Id);

        var loadTimer = Stopwatch.StartNew();

        var game = gameStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(gameInfo)).Result;

        var events = game.GetEvents().ToArray();

        var stateStore = stateStoreFactory();
        var keyFrameService = keyFrameServiceFactory(stateStore);

        if (keyFrameSettings.Enabled)
        {
            stateStore.EventHandled += (_, e) =>
            {
                if (e.Index % keyFrameSettings.KeyFrameFrequency == 0)
                {
                    keyFrameService.CaptureKeyFrameAtTick(e.Tick);
                }
            };
        }

        var reducerGameContext = new ReducerGameContext(gameInfo, stateStore);
        var reducers = 
            reducerFactories.Select(f => f(reducerGameContext))
                .ValidateDependencies()
                .SortReducers()
                .ToImmutableList();
        stateStore.LoadDefaultStates(reducers);
        stateStore.ApplyEvents(reducers, null, events);

        var gameClock = gameClockFactory(gameInfo, reducers.OfType<ITickReceiver>());
        gameClock.Run();

        loadTimer.Stop();
        logger.LogInformation("Loaded game in {loadTime}ms", loadTimer.ElapsedMilliseconds);

        return new GameContext(gameInfo, reducers, stateStore, gameClock, keyFrameService);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
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
    IGameClock GameClock,
    IKeyFrameService KeyFrameService) : IDisposable
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        GameClock.Dispose();
    }
}