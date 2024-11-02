using System.Collections.Concurrent;
using amethyst.DataStores;
using amethyst.Services;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Hubs;

public class GameStoreHub : Hub
{
    private readonly IGameDiscoveryService _gameDiscoveryService;
    private readonly ILogger<GameStoreHub> _logger;
    private readonly ConcurrentBag<Func<IEnumerable<GameInfo>, Task>> _gameListWatchers = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public GameStoreHub(IGameDiscoveryService gameDiscoveryService, ILogger<GameStoreHub> logger)
    {
        _gameDiscoveryService = gameDiscoveryService;
        _logger = logger;

        RunWatchThread();
    }

    public void WatchGamesList()
    {
        var caller = Clients.Caller;

        lock (_gameListWatchers)
        {
            _gameListWatchers.Add(games => caller.SendAsync("GamesListChanged", games));
        }
    }

    private void RunWatchThread()
    {
        new TaskFactory(_cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskContinuationOptions.None, TaskScheduler.Current)
            .StartNew(() =>
            {
                var cancellationToken = _cancellationTokenSource.Token;
                var games = _gameDiscoveryService.GetGames().ToArray();

                while (!cancellationToken.IsCancellationRequested)
                {
                    Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                    var updatedGames = _gameDiscoveryService.GetGames().ToArray();

                    if (!games.SequenceEqual(updatedGames))
                    {
                        games = updatedGames;

                        lock (_gameListWatchers)
                        {
                            foreach (var watcher in _gameListWatchers)
                            {
                                watcher(games);
                            }
                        }
                    }
                }
            });
    }

    protected override void Dispose(bool disposing)
    {
        _cancellationTokenSource.Cancel();
        base.Dispose(disposing);
    }
}