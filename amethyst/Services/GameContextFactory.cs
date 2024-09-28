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
    private readonly Dictionary<Guid, GameContext> _gameContexts = [];

    public GameContext GetGame(GameInfo gameInfo)
    {
        lock (_gameContexts)
        {
            if (!_gameContexts.ContainsKey(gameInfo.Id))
                LoadGame(gameInfo);
        }

        return _gameContexts[gameInfo.Id];
    }

    private void LoadGame(GameInfo gameInfo)
    {
        using var game = gameStoreFactory(IGameDiscoveryService.GetGameFileName(gameInfo));

        var events = game.GetEvents().ToArray();

        var stateStore = stateStoreFactory();
        var gameContextWithoutReducers = new GameContext(gameInfo, [], stateStore);
        var reducers = reducerFactories.Select(f => f(gameContextWithoutReducers)).ToImmutableList();
        stateStore.LoadDefaultStates(reducers);
        stateStore.ApplyEvents(reducers, events);

        _gameContexts[gameInfo.Id] = gameContextWithoutReducers with { Reducers = reducers };

        gameClockFactory(reducers.OfType<ITickReceiver>()).Run();
    }
}

public record GameContext(GameInfo GameInfo, IImmutableList<IReducer> Reducers, IGameStateStore StateStore);