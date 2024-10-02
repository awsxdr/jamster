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
    Func<IEnumerable<ITickReceiver>, IGameClock> gameClockFactory,
    GameStoreFactory gameStoreFactory) 
    : IGameContextFactory
{
    private readonly ConcurrentDictionary<Guid, GameContext> _gameContexts = [];

    public GameContext GetGame(GameInfo gameInfo) =>
        _gameContexts.GetOrAdd(gameInfo.Id, _ => LoadGame(gameInfo));

    private GameContext LoadGame(GameInfo gameInfo)
    {
        using var game = gameStoreFactory(IGameDiscoveryService.GetGameFileName(gameInfo));

        var events = game.GetEvents().ToArray();

        var stateStore = stateStoreFactory();
        var gameContextWithoutReducers = new GameContext(gameInfo, [], stateStore);
        var reducers = reducerFactories.Select(f => f(gameContextWithoutReducers)).ToImmutableList();
        stateStore.LoadDefaultStates(reducers);
        stateStore.ApplyEvents(reducers, events);

        gameClockFactory(reducers.OfType<ITickReceiver>()).Run();

        return gameContextWithoutReducers with {Reducers = reducers};
    }
}

public record GameContext(GameInfo GameInfo, IImmutableList<IReducer> Reducers, IGameStateStore StateStore);