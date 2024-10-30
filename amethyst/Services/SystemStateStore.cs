using amethyst.DataStores;
using Func;

namespace amethyst.Services;

public interface ISystemStateStore
{
    event EventHandler<SystemStateStore.SystemStateChangedEventArgs<Guid>>? CurrentGameChanged;
    Result<GameInfo> GetCurrentGame();
    Result<GameInfo> SetCurrentGame(Guid gameId);
}

public class SystemStateStore(ISystemStateDataStore dataStore, IGameDiscoveryService gameDiscoveryService) : ISystemStateStore
{
    public event EventHandler<SystemStateChangedEventArgs<Guid>>? CurrentGameChanged;

    public Result<GameInfo> GetCurrentGame() =>
        dataStore.GetCurrentGame()
            .Then(gameDiscoveryService.GetExistingGame);

    public Result<GameInfo> SetCurrentGame(Guid gameId) =>
        gameDiscoveryService.GetExistingGame(gameId)
            .Then(game =>
            {
                dataStore.SetCurrentGame(gameId);
                CurrentGameChanged?.Invoke(this, new(gameId));

                return Result.Succeed(game);
            });

    public sealed class SystemStateChangedEventArgs<TValue>(TValue value) : EventArgs
    {
        public TValue Value { get; } = value;
    }

}