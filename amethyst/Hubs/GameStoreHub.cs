using amethyst.Services;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Hubs;

public class GameStoreNotifier : Notifier<GameStoreHub>, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IGameDiscoveryService _gameDiscoveryService;

    public override string HubAddress => "api/hubs/games";

    public GameStoreNotifier(
        IGameDiscoveryService gameDiscoveryService,
        IHubContext<GameStoreHub> hubContext)
        : base(hubContext)
    {
        _gameDiscoveryService = gameDiscoveryService;

        RunWatchThread();
    }

    private void RunWatchThread()
    {
        new TaskFactory(_cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskContinuationOptions.None, TaskScheduler.Current)
            .StartNew<Task>(async () =>
            {
                var cancellationToken = _cancellationTokenSource.Token;
                var games = await _gameDiscoveryService.GetGames();

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                    var updatedGames = await _gameDiscoveryService.GetGames();

                    if (!games.SequenceEqual(updatedGames))
                    {
                        games = updatedGames;

                        await HubContext.Clients.Group("GameList").SendAsync("GamesListChanged", games, cancellationToken);
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