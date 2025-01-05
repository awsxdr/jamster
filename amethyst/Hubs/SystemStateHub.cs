using amethyst.Services;
using Func;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Hubs;

public class SystemStateNotifier : Notifier<SystemStateHub>
{
    public SystemStateNotifier(
        ISystemStateStore systemStateStore,
        IGameDiscoveryService gameDiscoveryService,
        IHubContext<SystemStateHub> hubContext,
        ILogger<SystemStateNotifier> logger
        )
        : base(hubContext)
    {
        systemStateStore.CurrentGameChanged += async (_, e) =>
        {
            logger.LogDebug("Notifying client of current game change");

            await gameDiscoveryService.GetExistingGame(e.Value)
                .Then(async gameInfo =>
                {
                    await hubContext.Clients.Group("CurrentGame").SendAsync("CurrentGameChanged", gameInfo);
                    return Result.Succeed();
                })
                .OnError<GameFileNotFoundForIdError>(_ =>
                {
                    logger.LogError("Could not find game file for new current game");
                });
        };
    }
}

public class SystemStateHub : Hub
{
    public async Task WatchSystemState()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "CurrentGame");
    }
}