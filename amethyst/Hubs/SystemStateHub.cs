using amethyst.Services;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Hubs;

public class SystemStateHub(ISystemStateStore systemStateStore) : Hub
{
    public void WatchSystemState()
    {
        var caller = Clients.Caller;

        systemStateStore.CurrentGameChanged += (_, e) =>
        {
            caller.SendCoreAsync("CurrentGameChanged", [e.Value]);
        };
    }
}