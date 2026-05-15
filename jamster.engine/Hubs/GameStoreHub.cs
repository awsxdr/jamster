using jamster.engine.Services;

using Microsoft.AspNetCore.SignalR;

namespace jamster.engine.Hubs;

public class GameStoreNotifier : Notifier<GameStoreHub>, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public override string HubAddress => "api/hubs/games";

    public GameStoreNotifier(
        IGameDiscoveryService gameDiscoveryService,
        IHubContext<GameStoreHub> hubContext)
        : base(hubContext)
    {
        gameDiscoveryService.GamesListChanged += (_, games) =>
        {
            HubContext.Clients.Group("GameList").SendAsync("GamesListChanged", games, _cancellationTokenSource.Token);
        };
    }

    public void Dispose() =>
        _cancellationTokenSource.Cancel();
}

public class GameStoreHub : Hub
{
    public Task WatchGamesList() =>
        Groups.AddToGroupAsync(Context.ConnectionId, "GameList");
}