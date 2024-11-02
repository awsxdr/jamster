using amethyst.Services;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Hubs;

public class GameStoreNotifier : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IGameDiscoveryService _gameDiscoveryService;
    private readonly IHubContext<GameStoreHub> _hubContext;

    public GameStoreNotifier(
        IGameDiscoveryService gameDiscoveryService,
        IHubContext<GameStoreHub> hubContext)
    {
        _gameDiscoveryService = gameDiscoveryService;
        _hubContext = hubContext;

        RunWatchThread();
    }

    private void RunWatchThread()
    {
        new TaskFactory(_cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskContinuationOptions.None, TaskScheduler.Current)
            .StartNew(async () =>
            {
                var cancellationToken = _cancellationTokenSource.Token;
                var games = _gameDiscoveryService.GetGames().ToArray();

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                    var updatedGames = _gameDiscoveryService.GetGames().ToArray();

                    if (!games.SequenceEqual(updatedGames))
                    {
                        games = updatedGames;

                        await _hubContext.Clients.Group("GameList").SendAsync("GamesListChanged", games, cancellationToken);
                    }
                }
            });
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
    }
}

public class GameStoreHub : Hub
{
    public async Task WatchGamesList()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "GameList");
    }
}