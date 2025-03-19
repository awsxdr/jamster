using amethyst.DataStores;

namespace amethyst.Services;

public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e);

public interface ISystemStateStore
{
    event AsyncEventHandler<SystemStateChangedEventArgs<Guid>>? CurrentGameChanged;
    Task<Result<GameInfo>> GetCurrentGame();
    Task<Result<GameInfo>> SetCurrentGame(Guid gameId);

    public sealed class SystemStateChangedEventArgs<TValue>(TValue value) : EventArgs
    {
        public TValue Value { get; } = value;
    }
}

public class SystemStateStore(ISystemStateDataStore dataStore, IGameDiscoveryService gameDiscoveryService) : ISystemStateStore
{
    public event AsyncEventHandler<ISystemStateStore.SystemStateChangedEventArgs<Guid>>? CurrentGameChanged;

    public Task<Result<GameInfo>> GetCurrentGame() =>
        dataStore.GetCurrentGame()
            .Then(gameDiscoveryService.GetExistingGame);

    public Task<Result<GameInfo>> SetCurrentGame(Guid gameId) =>
        gameDiscoveryService.GetExistingGame(gameId)
            .OnSuccess(game =>
                dataStore.SetCurrentGame(game.Id)
                    .OnSuccess(() => CurrentGameChanged.InvokeHandlersAsync(this, new ISystemStateStore.SystemStateChangedEventArgs<Guid>(gameId)))
            );
}