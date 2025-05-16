using System.Collections.Concurrent;
using amethyst.Domain;
using amethyst.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Services;

public interface IConnectedClientsService
{
    event AsyncEventHandler<ConnectedClientsChangedArgs>? ConnectedClientsChanged;

    Task<Result<(ConnectedClient Client, Guid Id)>> RegisterClient(string connectionId, string clientIp);
    Task<Result<ConnectedClient>> ReconnectClient(Guid clientId, string connectionId, string clientIp);
    Task<Result> UnregisterClient(Guid clientId);
    Task<Result> SetClientActivity(Guid clientId, ActivityData activity);
    Task<Result> RequestClientActivityChange(string clientName, ActivityData activity);
    Task<Result> SetClientName(string currentName, string newName);
    IEnumerable<ConnectedClient> GetConnectedClients();
    Result<ConnectedClient> GetClientById(Guid clientId);
    Result<ConnectedClient> GetClientByName(string clientName);

    public sealed class ConnectedClientsChangedArgs : EventArgs
    {
        public required ConnectedClient[] Clients { get; init; }
    }
}

[Singleton]
public class ConnectedClientsService(IHubContext<ConnectedClientsHub> clientsHub, ILogger<ConnectedClientsService> logger) : IConnectedClientsService
{
    private readonly ConcurrentDictionary<Guid, ConnectedClient> _connectedClients = new();
    private readonly ConcurrentDictionary<Guid, DisconnectedClient> _disconnectedClients = new();

    private readonly ConcurrentQueue<string> _availableClientNames = new(
        NameGenerator.NameNouns.SelectMany(noun =>
                NameGenerator.NameAdjectives.Select(adjective =>
                    $"{adjective.ToLowerInvariant()}-{noun.ToLowerInvariant()}"
                ))
            .ToList()
            .Shuffle());

    public event AsyncEventHandler<IConnectedClientsService.ConnectedClientsChangedArgs>? ConnectedClientsChanged;

    public async Task<Result<(ConnectedClient Client, Guid Id)>> RegisterClient(string connectionId, string clientIp)
    {
        ExpireDisconnectedClients();

        if (_connectedClients.Any(c => c.Value.ConnectionId == connectionId))
            return Result<(ConnectedClient Client, Guid Id)>.Fail<ConnectionIdAlreadyRegisteredError>();

        var clientId = Guid.NewGuid();

        _connectedClients[clientId] = CreateNewClient(connectionId, clientIp);

        await ConnectedClientsChanged.InvokeHandlersAsync(this, new() { Clients = _connectedClients.Values.ToArray() });

        return Result.Succeed((_connectedClients[clientId], clientId));
    }

    public async Task<Result<ConnectedClient>> ReconnectClient(Guid clientId, string connectionId, string clientIp)
    {
        ExpireDisconnectedClients();

        if (_connectedClients.TryGetValue(clientId, out var existingClient))
            return Result.Succeed(existingClient);

        if (!_disconnectedClients.TryRemove(clientId, out var client))
            return Result<ConnectedClient>.Fail<ClientNotFoundError>();

        var newClient = new ConnectedClient(connectionId, client.Name, clientIp, new UnknownActivity(), DateTimeOffset.UtcNow);

        _connectedClients[clientId] = newClient;

        await ConnectedClientsChanged.InvokeHandlersAsync(this, new() { Clients = _connectedClients.Values.ToArray() });

        return Result.Succeed(newClient);
    }

    public async Task<Result> UnregisterClient(Guid clientId)
    {
        ExpireDisconnectedClients();

        if (!_connectedClients.TryRemove(clientId, out var client))
            return Result.Fail<ClientNotFoundError>();

        _disconnectedClients[clientId] = new(client.Name, DateTimeOffset.UtcNow);

        await ConnectedClientsChanged.InvokeHandlersAsync(this, new() { Clients = _connectedClients.Values.ToArray() });

        return Result.Succeed();
    }

    public async Task<Result> SetClientActivity(Guid clientId, ActivityData activity)
    {
        if (!_connectedClients.TryGetValue(clientId, out var client))
            return Result.Fail<ClientNotFoundError>();

        _connectedClients[clientId] = client with { ActivityInfo = activity };

        await ConnectedClientsChanged.InvokeHandlersAsync(this, new() { Clients = _connectedClients.Values.ToArray() });

        return Result.Succeed();
    }

    public async Task<Result> RequestClientActivityChange(string clientName, ActivityData activity)
    {
        var client = _connectedClients.Values.FirstOrDefault(c => c.Name.Name.Equals(clientName, StringComparison.OrdinalIgnoreCase));

        if(client == null)
            return Result.Fail<ClientNotFoundError>();

        await clientsHub.Clients.Client(client.ConnectionId).SendAsync("ChangeActivity", activity);

        return Result.Succeed();
    }

    public async Task<Result> SetClientName(string currentName, string newName)
    {
        ExpireDisconnectedClients();

        var existingDisconnectedClient = _disconnectedClients.FirstOrDefault(c => c.Value.Name.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));

        if (existingDisconnectedClient.Value is not null)
            _disconnectedClients.Remove(existingDisconnectedClient.Key, out _);

        if (_connectedClients.Any(c => c.Value.Name.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            return Result.Fail<ClientNameAlreadyInUseError>();

        var client = _connectedClients.FirstOrDefault(c => c.Value.Name.Name.Equals(currentName));

        if (client.Value == null)
            return Result.Fail<ClientNotFoundError>();

        _connectedClients[client.Key] = client.Value with { Name = new(newName, true) };

        await ConnectedClientsChanged.InvokeHandlersAsync(this, new() { Clients = _connectedClients.Values.ToArray() });

        return Result.Succeed();
    }

    public IEnumerable<ConnectedClient> GetConnectedClients() => _connectedClients.Values;

    public Result<ConnectedClient> GetClientById(Guid clientId) =>
        _connectedClients.TryGetValue(clientId, out var client)
            ? Result.Succeed(client)
            : Result<ConnectedClient>.Fail<ClientNotFoundError>();

    public Result<ConnectedClient> GetClientByName(string clientName) =>
        _connectedClients.Values.FirstOrDefault(c => c.Name.Name.Equals(clientName, StringComparison.OrdinalIgnoreCase))
            ?.Map(Result.Succeed)
            ?? Result<ConnectedClient>.Fail<ClientNotFoundError>();

    private ConnectedClient CreateNewClient(string connectionId, string clientIp) =>
        new(connectionId, GenerateUniqueName(), clientIp, new UnknownActivity(), DateTimeOffset.UtcNow);

    private string GenerateUniqueName()
    {
        if (!_availableClientNames.TryDequeue(out var name))
            throw new NameListExhaustedException();

        return name;
    }

    private void ExpireDisconnectedClients()
    {
        var expiryTime = DateTimeOffset.UtcNow - TimeSpan.FromMinutes(5);

        var expiredClients = _disconnectedClients.Where(c => c.Value.DisconnectionTime < expiryTime);

        foreach (var expiredClient in expiredClients)
        {
            _disconnectedClients.TryRemove(expiredClient.Key, out _);
        }
    }
}

public sealed class NameListExhaustedException : Exception;
public sealed class ClientNotFoundError : NotFoundError;
public sealed class ConnectionIdAlreadyRegisteredError : ResultError;
public sealed class ClientNameAlreadyInUseError : ResultError;

public record ConnectedClient(string ConnectionId, ClientName Name, string IpAddress, ActivityData ActivityInfo, DateTimeOffset LastUpdateTime);
public record DisconnectedClient(ClientName Name, DateTimeOffset DisconnectionTime);

public record ClientName(string Name, bool IsCustom)
{
    public static implicit operator ClientName(string name) => new(name, false);
}

public enum ClientActivity
{
    Unknown,
    Scoreboard,
    StreamOverlay,
    PenaltyWhiteboard,
    ScoreboardOperator,
    PenaltyLineupControl,
    PenaltyControl,
    LineupControl,
    BoxTiming,
    Other,
}

public record ActivityData(ClientActivity Activity, string? GameId, string LanguageCode);
public record UnknownActivity() : ActivityData(ClientActivity.Unknown, null, "en");
public record ScoreboardActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.Scoreboard, GameId, LanguageCode);
public record StreamOverlayActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.StreamOverlay, GameId, LanguageCode);
public record PenaltyWhiteboardActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.PenaltyWhiteboard, GameId, LanguageCode);
public record ScoreboardOperatorActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.ScoreboardOperator, GameId, LanguageCode);
public record PenaltyLineupControlActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.PenaltyLineupControl, GameId, LanguageCode);
public record PenaltyControlActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.PenaltyControl, GameId, LanguageCode);
public record LineupControlActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.LineupControl, GameId, LanguageCode);
public record BoxTimingActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.BoxTiming, GameId, LanguageCode);
public record OtherActivity(string? GameId, string LanguageCode) : ActivityData(ClientActivity.Other, GameId, LanguageCode);
