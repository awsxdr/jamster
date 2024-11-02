using amethyst.Services;
using Func;
using Microsoft.AspNetCore.SignalR;
using Result = Func.Result;

namespace amethyst.Hubs;

public class SystemStateHub(
    ISystemStateStore systemStateStore,
    IGameDiscoveryService gameDiscoveryService,
    ILogger<SystemStateHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Test");

        await base.OnConnectedAsync();
    }

    public void WatchSystemState()
    {
        var caller = Clients.Caller;

        systemStateStore.CurrentGameChanged += async (_, e) =>
        {
            logger.LogDebug("Notifying client of current game change");

            await gameDiscoveryService.GetExistingGame(e.Value)
                .Then(async gameInfo =>
                {
                    await caller.SendAsync("CurrentGameChanged", gameInfo);
                    return Result.Succeed();
                })
                .OnError<GameFileNotFoundForIdError>(_ =>
                {
                    logger.LogError("Could not find game file for new current game");
                });
        };
    }
}