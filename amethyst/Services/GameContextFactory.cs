using System.Collections.Concurrent;
using System.Diagnostics;

namespace amethyst.Services;

using System.Collections.Immutable;
using amethyst.Domain;
using amethyst.Events;
using CommandLine;
using DataStores;
using Microsoft.Extensions.Logging;
using Reducers;

public interface IGameContextFactory : IDisposable
{
    GameContext GetGame(GameInfo gameInfo);
    void UnloadGame(Guid gameId);
    Task ReloadGame(GameInfo gameInfo);
    Task ApplyKeyFrame(GameInfo gameIndo, Tick tick, KeyFrame keyFrame);
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

        var context = _gameContexts[gameInfo.Id].Value;
        var stateStore = context.StateStore;
        stateStore.DisableNotifications();
        context.GameClock.Stop();
        stateStore.LoadDefaultStates(context.Reducers);

        var game = await gameStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(gameInfo));
        var events = game.GetEvents().ToArray();
        await stateStore.ApplyEvents(context.Reducers, events);

        stateStore.EnableNotifications();
        context.GameClock.Run();
        stateStore.ForceNotify();
    }

    public async Task ApplyKeyFrame(GameInfo gameInfo, Tick tick, KeyFrame keyFrame)
    {
        if (!_gameContexts.ContainsKey(gameInfo.Id) || !_gameContexts[gameInfo.Id].IsValueCreated)
            return;

        var context = _gameContexts[gameInfo.Id].Value;
        var stateStore = context.StateStore;
        stateStore.DisableNotifications();
        context.GameClock.Stop();
        stateStore.LoadDefaultStates(context.Reducers);

        var game = await gameStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(gameInfo));
        stateStore.ApplyKeyFrame(context.Reducers, keyFrame);
        context.KeyFrameService.ClearFramesAfter(tick);

        var gameDataStore = await gameStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(gameInfo));
        var subsequentEvents = gameDataStore.GetEvents().Where(e => e.Id.Tick >= tick).ToArray();

        await stateStore.ApplyEvents(context.Reducers, subsequentEvents);

        stateStore.EnableNotifications();
        context.GameClock.Run();
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
        stateStore.ApplyEvents(reducers, events);

        var gameClock = gameClockFactory(gameInfo, reducers.OfType<ITickReceiver>());
        gameClock.Run();

        loadTimer.Stop();
        logger.LogInformation("Loaded game in {loadTime}ms", loadTimer.ElapsedMilliseconds);

        return new GameContext(gameInfo, reducers, stateStore, gameClock, keyFrameService);
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
    IGameClock GameClock,
    IKeyFrameService KeyFrameService) : IDisposable
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);

        GameClock.Dispose();
    }
}