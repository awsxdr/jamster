using System.Collections.Concurrent;
using amethyst.Domain;
using amethyst.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace amethyst.Services;

public interface IConnectedClientsService
{
    event AsyncEventHandler<ConnectedClientsChangedArgs>? ConnectedClientsChanged;

    Task<string> RegisterClient(string clientId);
    Task UnregisterClient(string clientId);
    Task<Result> SetClientActivity(string clientId, ClientActivity activity, string path, string? gameId);
    Task<Result> RequestClientActivityChange(string clientId, ClientActivity activity, string? gameId);
    Task<Result> SetClientName(string clientId, string name);
    IEnumerable<ConnectedClient> GetConnectedClients();
    Result<ConnectedClient> GetClient(string connectionId);

    public sealed class ConnectedClientsChangedArgs : EventArgs
    {
        public required ConnectedClient[] Clients { get; init; }
    }
}

[Singleton]
public class ConnectedClientsService(IHubContext<ConnectedClientsHub> clientsHub, ILogger<ConnectedClientsService> logger) : IConnectedClientsService
{
    public event AsyncEventHandler<IConnectedClientsService.ConnectedClientsChangedArgs>? ConnectedClientsChanged;

    private readonly ConcurrentDictionary<string, ConnectedClient> _connectedClients = new();

    private readonly Queue<string> _availableClientNames = new(
        NameGenerator.NameNouns.SelectMany(noun =>
                NameGenerator.NameAdjectives.Select(adjective =>
                    $"{adjective.ToLowerInvariant()}-{noun.ToLowerInvariant()}"
                ))
            .ToList()
            .Shuffle());

    public async Task<string> RegisterClient(string clientId)
    {
        var client =
            _connectedClients.TryGetValue(clientId, out var existingClient)
                ? existingClient with { LastUpdateTime = DateTimeOffset.UtcNow }
                : CreateNewClient(clientId);
        _connectedClients.AddOrUpdate(clientId, client, (_, _) => client);

        await ConnectedClientsChanged.InvokeHandlersAsync(this, new() { Clients = _connectedClients.Values.ToArray() });

        return client.Name.Name;
    }

    public async Task UnregisterClient(string clientId)
    {
        if (!_connectedClients.Remove(clientId, out var client))
        {
            logger.LogWarning("Attempt to unregister a client which doesn't exist: {clientId}", clientId);
            return;
        }

        if (!client.Name.IsCustom)
            _availableClientNames.Enqueue(client.Name.Name);

        await ConnectedClientsChanged.InvokeHandlersAsync(this, new() { Clients = _connectedClients.Values.ToArray() });
    }

    public async Task<Result> SetClientActivity(string clientId, ClientActivity activity, string path, string? gameId)
    {
        if (!_connectedClients.TryGetValue(clientId, out var client))
        {
            logger.LogWarning("Attempt to set activity for a client which doesn't exist: {clientId}", clientId);
            return Result.Fail<ClientNotFoundError>();
        }

        _connectedClients[clientId] = client with
        {
            CurrentActivity = activity,
            Path = path,
            GameId = gameId,
            LastUpdateTime = DateTimeOffset.UtcNow
        };

        await ConnectedClientsChanged.InvokeHandlersAsync(this, new() { Clients = _connectedClients.Values.ToArray() });

        return Result.Succeed();
    }

    public async Task<Result> RequestClientActivityChange(string clientId, ClientActivity activity, string? gameId)
    {
        if (!_connectedClients.ContainsKey(clientId))
        {
            logger.LogWarning("Attempt to change activity for a client which doesn't exist: {clientId}", clientId);
            return Result.Fail<ClientNotFoundError>();
        }

        logger.LogDebug("Requesting client {clientId} changes activity to {activity} in game {gameId}", clientId, activity, gameId);

        await clientsHub.Clients.Client(clientId).SendAsync("ChangeActivity", activity, gameId);

        return Result.Succeed();
    }

    public async Task<Result> SetClientName(string clientId, string name)
    {
        if (!_connectedClients.TryGetValue(clientId, out var client))
        {
            logger.LogWarning("Attempt to set name for a client which doesn't exist: {clientId}", clientId);
            return Result.Fail<ClientNotFoundError>();
        }

        if (!client.Name.IsCustom)
            _availableClientNames.Enqueue(client.Name.Name);

        _connectedClients[clientId] = client with
        {
            Name = new(name, true),
        };

        await ConnectedClientsChanged.InvokeHandlersAsync(this, new() { Clients = _connectedClients.Values.ToArray() });

        return Result.Succeed();
    }

    public IEnumerable<ConnectedClient> GetConnectedClients() => _connectedClients.Values;

    public Result<ConnectedClient> GetClient(string connectionId) => 
        _connectedClients.TryGetValue(connectionId, out var client) 
            ? Result.Succeed(client)
            : Result<ConnectedClient>.Fail<ClientNotFoundError>();

    private ConnectedClient CreateNewClient(string clientId) =>
        new(clientId, GenerateUniqueName(), ClientActivity.Unknown, string.Empty, null, DateTimeOffset.UtcNow);

    private string GenerateUniqueName()
    {
        if (_availableClientNames.Count == 0)
            throw new NameListExhaustedException();

        return _availableClientNames.Dequeue();
    }
}

public sealed class NameListExhaustedException : Exception;
public sealed class ClientNotFoundError : NotFoundError;

public record ConnectedClient(string Id, ClientName Name, ClientActivity CurrentActivity, string Path, string? GameId, DateTimeOffset LastUpdateTime);

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