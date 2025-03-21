using amethyst.Services;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Hubs;

public class ConnectedClientsNotifier : Notifier<ConnectedClientsHub>
{
    private readonly IConnectedClientsService _connectedClientsService;
    public override string HubAddress => "api/hubs/clients";

    public ConnectedClientsNotifier(
        IConnectedClientsService connectedClientsService,
        IHubContext<ConnectedClientsHub> hubContext) 
        : base(hubContext)
    {
        _connectedClientsService = connectedClientsService;

        _connectedClientsService.ConnectedClientsChanged += OnConnectedClientsChanged;
    }

    private async Task OnConnectedClientsChanged(object? sender, IConnectedClientsService.ConnectedClientsChangedArgs e)
    {
        await HubContext.Clients.Groups("ClientsList").SendAsync("ConnectedClientsChanged", e.Clients);
    }
}

public class ConnectedClientsHub(IConnectedClientsService connectedClientsService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        try
        {
            Context.Items["friendlyName"] = await connectedClientsService.RegisterClient(Context.ConnectionId);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await connectedClientsService.UnregisterClient(Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

    public string GetConnectionName() => Context.Items["friendlyName"] as string ?? string.Empty;

    public void SetConnectionName(string connectionName)
    {
        connectedClientsService.SetClientName(Context.ConnectionId, connectionName);

        Context.Items["friendlyName"] = connectionName;
    }

    public ConnectedClient? GetConnectionDetails() =>
        connectedClientsService.GetClient(Context.ConnectionId) is Success<ConnectedClient> c ? c.Value : null;

    public async Task WatchClientsList()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "ClientsList");
    }
}