using System.Collections.Concurrent;
using System.Diagnostics;

namespace amethyst.Services;

using System.Collections.Immutable;
using DataStores;
using Reducers;

public interface IGameContextFactory
{
    GameContext GetGame(GameInfo gameInfo);
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

    private GameContext LoadGame(GameInfo gameInfo)
    {
        logger.LogInformation("Loading game state for {gameName} ({gameId})", gameInfo.Name, gameInfo.Id);

        var loadTimer = Stopwatch.StartNew();

        var game = gameStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(gameInfo));

        var events = game.GetEvents().ToArray();

        var stateStore = stateStoreFactory();
        var gameContextWithoutReducers = new GameContext(gameInfo, [], stateStore);
        var reducers = reducerFactories.Select(f => f(gameContextWithoutReducers)).ToImmutableList();
        stateStore.LoadDefaultStates(reducers);
        stateStore.ApplyEvents(reducers, events);

        gameClockFactory(gameInfo, reducers.OfType<ITickReceiver>()).Run();

        loadTimer.Stop();
        logger.LogInformation("Loaded game in {loadTime}ms", loadTimer.ElapsedMilliseconds);

        return gameContextWithoutReducers with {Reducers = reducers};
    }
}

public record GameContext(GameInfo GameInfo, IImmutableList<IReducer> Reducers, IGameStateStore StateStore);