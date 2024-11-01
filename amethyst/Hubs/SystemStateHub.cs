using amethyst.Services;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Hubs;

public class SystemStateHub(ISystemStateStore systemStateStore, ILogger<SystemStateHub> logger) : Hub
{
    public void WatchSystemState()
    {
        var caller = Clients.Caller;

        systemStateStore.CurrentGameChanged += async (_, e) =>
        {
            logger.LogDebug("Notifying client of current game change");
            await caller.SendAsync("CurrentGameChanged", e.Value);
        };
    }
}