using System.Collections.Concurrent;

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
    GameStoreFactory gameStoreFactory) 
    : IGameContextFactory
{
    private readonly ConcurrentDictionary<Guid, Lazy<GameContext>> _gameContexts = [];

    public GameContext GetGame(GameInfo gameInfo) =>
        // GetOrAdd is not thread-safe. The use of Lazy<> ensures that LoadGame only gets called once.
        _gameContexts.GetOrAdd(gameInfo.Id, _ => new(() => LoadGame(gameInfo))).Value;

    private GameContext LoadGame(GameInfo gameInfo)
    {
        using var game = gameStoreFactory(IGameDiscoveryService.GetGameFileName(gameInfo));

        var events = game.GetEvents().ToArray();

        var stateStore = stateStoreFactory();
        var gameContextWithoutReducers = new GameContext(gameInfo, [], stateStore);
        var reducers = reducerFactories.Select(f => f(gameContextWithoutReducers)).ToImmutableList();
        stateStore.LoadDefaultStates(reducers);
        stateStore.ApplyEvents(reducers, events);

        gameClockFactory(gameInfo, reducers.OfType<ITickReceiver>()).Run();

        return gameContextWithoutReducers with {Reducers = reducers};
    }
}

public record GameContext(GameInfo GameInfo, IImmutableList<IReducer> Reducers, IGameStateStore StateStore);