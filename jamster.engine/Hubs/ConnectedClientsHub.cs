using System.Text.Json;
using System.Text.Json.Nodes;
using jamster.Controllers;
using jamster.Domain;
using jamster.Services;
using Microsoft.AspNetCore.SignalR;

namespace jamster.Hubs;

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
        await HubContext.Clients.Groups("ClientsList").SendAsync("ConnectedClientsChanged", e.Clients.Select(c => (ClientsController.ClientModel)c).ToArray());
    }
}

public class ConnectedClientsHub(IConnectedClientsService connectedClientsService) : Hub
{
    private const string ClientIdKey = "client-id";

    public override async Task OnConnectedAsync()
    {
        var result = await connectedClientsService.RegisterClient(Context.ConnectionId, Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? "?.?.?.?");

        if (result is Failure<ConnectionIdAlreadyRegisteredError>)
        {
            var clientId = GetClientId();

            if (clientId == null)
                throw new UnableToRegisterClientException();

            result =
                await connectedClientsService.ReconnectClient(clientId.Value, Context.ConnectionId, Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? "?.?.?.?")
                    .ThenMap(c => (c, clientId.Value));
        }

        var id = result switch
        {
            Success<(ConnectedClient, Guid Id)> s => s.Value.Id,
            Failure f => throw new FailedToRegisterClientException { Error = f.GetError() },
            _ => throw new UnableToRegisterClientException()
        };

        Context.Items[ClientIdKey] = id;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var clientId = GetClientId();

        if (clientId == null)
            return;

        await connectedClientsService.UnregisterClient(clientId.Value);

        await base.OnDisconnectedAsync(exception);
    }

    public string GetConnectionName()
    {
        var clientId = GetClientId();

        if (clientId == null)
            return string.Empty;

        return connectedClientsService.GetClientById(clientId.Value) is Success<ConnectedClient> s
            ? s.Value.Name.Name
            : string.Empty;
    }

    public async Task SetConnectionName(string connectionName)
    {
        var clientId = GetClientId();

        if (clientId == null) 
            return;

        var client = connectedClientsService.GetClientById(clientId.Value) switch
        {
            Success<ConnectedClient> s => s.Value,
            Failure failure => throw new FailedToSetNameException { Error = failure.GetError() },
            _ => throw new UnableToSetNameException()
        };

        var result = await connectedClientsService.SetClientName(client.Name.Name, connectionName);

        if (result is Failure f)
            throw new FailedToSetNameException { Error = f.GetError() };
    }

    public ClientsController.ClientModel? GetConnectionDetails()
    {
        var clientId = GetClientId();

        if (clientId == null)
            return null;

        return connectedClientsService.GetClientById(clientId.Value) is Success<ConnectedClient> c
            ? (ClientsController.ClientModel)c.Value
            : null;
    }

    public async Task SetActivity(JsonObject activity)
    {
        var clientId = GetClientId();

        if (clientId == null)
            return;

        ActivityData? activityDetails = activity[nameof(ActivityData.Activity)]?.AsValue().Deserialize<ClientActivity>(Program.JsonSerializerOptions) switch
        {
            ClientActivity.Scoreboard => activity.Deserialize<ScoreboardActivity>(Program.JsonSerializerOptions),
            ClientActivity.StreamOverlay => activity.Deserialize<StreamOverlayActivity>(Program.JsonSerializerOptions),
            _ => activity.Deserialize<UnknownActivity>(Program.JsonSerializerOptions)!
        };

        if (activityDetails == null)
            throw new ActivityDetailsFormatException();

        await connectedClientsService.SetClientActivity(clientId.Value, activityDetails);
    }

    public Task WatchClientsList() =>
        Groups.AddToGroupAsync(Context.ConnectionId, "ClientsList");

    private Guid? GetClientId() => Context.Items[ClientIdKey] as Guid?;
}

public sealed class UnableToRegisterClientException : Exception;

public sealed class FailedToRegisterClientException : Exception
{
    public required ResultError Error { get; init; }
}

public sealed class UnableToSetNameException : Exception;

public sealed class FailedToSetNameException : Exception
{
    public required ResultError Error { get; init; }
}

public sealed class ActivityDetailsFormatException : Exception;