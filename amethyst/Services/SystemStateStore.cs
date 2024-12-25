using amethyst.DataStores;
using amethyst.Extensions;
using Func;

namespace amethyst.Services;

public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs e);

public interface ISystemStateStore
{
    event AsyncEventHandler<SystemStateStore.SystemStateChangedEventArgs<Guid>>? CurrentGameChanged;
    Task<Result<GameInfo>> GetCurrentGame();
    Task<Result<GameInfo>> SetCurrentGame(Guid gameId);
}

public class SystemStateStore(ISystemStateDataStore dataStore, IGameDiscoveryService gameDiscoveryService) : ISystemStateStore
{
    public event AsyncEventHandler<SystemStateChangedEventArgs<Guid>>? CurrentGameChanged;

    public Task<Result<GameInfo>> GetCurrentGame() =>
        dataStore.GetCurrentGame()
            .Then(gameDiscoveryService.GetExistingGame);

    public Task<Result<GameInfo>> SetCurrentGame(Guid gameId) =>
        gameDiscoveryService.GetExistingGame(gameId)
            .Then(async game =>
            {
                dataStore.SetCurrentGame(gameId);
                await CurrentGameChanged.InvokeHandlersAsync(this, new SystemStateChangedEventArgs<Guid>(gameId));

                return Result.Succeed(game);
            });

    public sealed class SystemStateChangedEventArgs<TValue>(TValue value) : EventArgs
    {
        public TValue Value { get; } = value;
    }

}