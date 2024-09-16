namespace amethyst.Services;

using DataStores;

public interface IGameStateStoreFactory
{
    IGameStateStore GetGame(GameInfo gameInfo);
}

public class GameStateStoreFactory(Func<IGameStateStore> stateStoreFactory, GameStoreFactory gameStoreFactory) : IGameStateStoreFactory
{
    private readonly Dictionary<Guid, IGameStateStore> _gameStateStores = [];

    public IGameStateStore GetGame(GameInfo gameInfo)
    {
        if (!_gameStateStores.ContainsKey(gameInfo.Id))
            LoadGame(gameInfo);

        return _gameStateStores[gameInfo.Id];
    }

    private void LoadGame(GameInfo gameInfo)
    {
        using var game = gameStoreFactory(IGameDiscoveryService.GetGameFileName(gameInfo));

        var events = game.GetEvents().ToArray();

        var stateStore = stateStoreFactory();
        stateStore.ApplyEvents(events);

        _gameStateStores[gameInfo.Id] = stateStore;
    }
}