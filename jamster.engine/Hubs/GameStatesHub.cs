using System.Collections.Concurrent;

using DotNext.Threading;

using jamster.engine.Services;

using Microsoft.AspNetCore.SignalR;

namespace jamster.engine.Hubs;

public class GameStatesNotifier(IGameDiscoveryService gameDiscoveryService, IHubContext<GameStatesHub> hubContext, IGameContextFactory contextFactory)
    : Notifier<GameStatesHub>(hubContext)
{
    private readonly ConcurrentDictionary<Guid, List<string>> _watchedStatesByGame = new();
    private readonly AsyncManualResetEvent _watchedStatesLock = new(false);

    public override string HubAddress => "api/hubs/game/{gameId:guid}";

    public async Task WatchStateName(Guid gameId, string stateName)
    {
        var watchedStates = _watchedStatesByGame.GetOrAdd(gameId, _ => new());

        using var @lock = await _watchedStatesLock.AcquireLockAsync();

        if (watchedStates.Contains(stateName)) return;

        watchedStates.Add(stateName);

        var gameContext = await GetGameContext(gameId);

        gameContext.StateStore.WatchStateByName(
            stateName,
            async state =>
            {
                var group = HubContext.Clients.Group($"{gameId}_{stateName}");
                
                await group.SendAsync("StateChanged", stateName, state);
            });
    }

    private async Task<GameContext> GetGameContext(Guid gameId) =>
        await gameDiscoveryService.GetExistingGame(gameId)
                .ThenMap(contextFactory.GetGame)
            switch
            {
                Success<GameContext> context => context.Value,
                _ => throw new GameNotFoundHubException()
            };

    public class GameNotFoundHubException : HubException;
}

public class GameStatesHub(
    GameStatesNotifier notifier,
    IGameDiscoveryService gameDiscoveryService
    ) : Hub
{
    public async Task WatchState(string stateName)
    {
        var gameId = GetGameId();

        await Groups.AddToGroupAsync(Context.ConnectionId, $"{gameId}_{stateName}");

        await notifier.WatchStateName(gameId, stateName);
    }

    public async Task UnwatchState(string stateName)
    {
        var gameId = GetGameId();

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{gameId}_{stateName}");
    }

    private Guid GetGameId()
    {
        var gameId = Context.GetHttpContext()?.GetRouteValue("gameId") as string;

        if (
            gameId is null
            || !Guid.TryParse(gameId, out var gameIdGuid)
            || !gameDiscoveryService.GameExists(gameIdGuid))
        {
            throw new GameNotFoundHubException();
        }

        return gameIdGuid;
    }

    public class GameNotFoundHubException : HubException;
}