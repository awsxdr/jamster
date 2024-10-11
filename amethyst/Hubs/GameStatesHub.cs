using amethyst.Services;
using Func;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Hubs;

public class GameStatesHub(IGameDiscoveryService gameDiscoveryService, IGameContextFactory contextFactory) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var gameId = GetGameId();

        await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
        
        await base.OnConnectedAsync();
    }

    public Task WatchState(string stateName)
    {
        var gameId = GetGameId();
        var gameContext = GetGameContext(gameId);
        var caller = Clients.Caller;

        gameContext.StateStore.WatchStateByName(stateName, async state =>
        {
            await caller.SendCoreAsync("StateChanged", [stateName, state]);
        });

        return Task.CompletedTask;
    }

    private GameContext GetGameContext(Guid gameId) =>
        gameDiscoveryService.GetExistingGame(gameId)
                .ThenMap(contextFactory.GetGame)
            switch
            {
                Success<GameContext> context => context.Value,
                _ => throw new GameNotFoundHubException()
            };

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