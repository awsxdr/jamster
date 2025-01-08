using Microsoft.AspNetCore.SignalR;

namespace amethyst.Hubs;

public interface INotifier
{
    string HubAddress { get; }
    Type HubType { get; }
}

public abstract class Notifier<THub>(IHubContext<THub> hubContext) : INotifier where THub : Hub
{
    public abstract string HubAddress { get; }
    Type INotifier.HubType => typeof(THub);

    protected IHubContext<THub> HubContext { get; } = hubContext;
}

public abstract class Notifier<THub, TClient>(IHubContext<THub, TClient> hubContext)
    : INotifier
    where THub : Hub<TClient>
    where TClient : class
{
    public abstract string HubAddress { get; }
    Type INotifier.HubType => typeof(THub);

    protected IHubContext<THub, TClient> HubContext { get; } = hubContext;
}