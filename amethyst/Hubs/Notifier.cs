using Microsoft.AspNetCore.SignalR;

namespace amethyst.Hubs;

public interface INotifier;

public abstract class Notifier<THub>(IHubContext<THub> hubContext) : INotifier where THub : Hub
{
    protected IHubContext<THub> HubContext { get; } = hubContext;
}

public abstract class Notifier<THub, TClient>(IHubContext<THub, TClient> hubContext)
    : INotifier
    where THub : Hub<TClient>
    where TClient : class
{
    protected IHubContext<THub, TClient> HubContext { get; } = hubContext;
}