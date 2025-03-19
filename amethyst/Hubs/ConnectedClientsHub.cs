using amethyst.Services;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Hubs;

public class ConnectionClientsNotifier(IHubContext<ConnectedClientsHub> hubContext) : Notifier<ConnectedClientsHub>(hubContext)
{
    public override string HubAddress => "api/hubs/clients";
}

public class ConnectedClientsHub(IConnectedClientsService connectedClientsService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        Context.Items["friendlyName"] = connectedClientsService.RegisterClient(Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        connectedClientsService.UnregisterClient(Context.ConnectionId);
    }

    public string GetConnectionName() => Context.Items["friendlyName"] as string ?? string.Empty;

    public void SetConnectionName(string connectionName)
    {
        connectedClientsService.SetClientName(Context.ConnectionId, connectionName);

        Context.Items["friendlyName"] = connectionName;
    }
}