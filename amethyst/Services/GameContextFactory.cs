namespace amethyst.Services;

using System.Collections.Immutable;
using DataStores;
using Reducers;

public interface IGameContextFactory
{
    GameContext GetGame(GameInfo gameInfo);
}

public class GameContextFactory(
    Func<IGameStateStore> stateStoreFactory, 
    IImmutableList<ReducerFactory> reducerFactories, 
    GameStoreFactory gameStoreFactory) : IGameContextFactory
{
    private readonly Dictionary<Guid, GameContext> _gameContexts = [];

    public GameContext GetGame(GameInfo gameInfo)
    {
        if (!_gameContexts.ContainsKey(gameInfo.Id))
            LoadGame(gameInfo);

        return _gameContexts[gameInfo.Id];
    }

    private void LoadGame(GameInfo gameInfo)
    {
        using var game = gameStoreFactory(IGameDiscoveryService.GetGameFileName(gameInfo));

        var events = game.GetEvents().ToArray();

        var stateStore = stateStoreFactory();
        var reducers = reducerFactories.Select(f => f(stateStore)).ToImmutableList();
        stateStore.LoadDefaultStates(reducers);
        stateStore.ApplyEvents(reducers, events);

        _gameContexts[gameInfo.Id] = new(reducers, stateStore);
    }
}

public record GameContext(IImmutableList<IReducer> Reducers, IGameStateStore StateStore);