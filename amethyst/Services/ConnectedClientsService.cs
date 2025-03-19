namespace amethyst.Services;

public interface IConnectedClientsService
{
    event EventHandler<ConnectedClientsChangedArgs>? ConnectedClientsChanged;
    string RegisterClient(string clientId);
    void UnregisterClient(string clientId);
    void SetClientActivity(string clientId, ClientActivity activity, string path);
    void SetClientName(string clientId, string name);
    IEnumerable<ConnectedClient> GetConnectedClients();

    public sealed class ConnectedClientsChangedArgs : EventArgs
    {
        public required ConnectedClient[] Clients { get; init; }
    }
}

[Singleton]
public class ConnectedClientsService : IConnectedClientsService
{
    public event EventHandler<IConnectedClientsService.ConnectedClientsChangedArgs>? ConnectedClientsChanged;

    private readonly Dictionary<string, ConnectedClient> _connectedClients = new();

    private readonly Queue<string> _availableClientNames = new(
        NameGenerator.NameNouns.SelectMany(noun =>
                NameGenerator.NameAdjectives.Select(adjective =>
                    $"{adjective.ToLowerInvariant()}-{noun.ToLowerInvariant()}"
                ))
            .ToList()
            .Shuffle());

    public string RegisterClient(string clientId)
    {
        var client =
            _connectedClients.TryGetValue(clientId, out var existingClient)
                ? existingClient with { LastUpdateTime = DateTimeOffset.UtcNow }
                : CreateNewClient(clientId);
        _connectedClients[clientId] = client;

        ConnectedClientsChanged?.Invoke(this, new() { Clients = _connectedClients.Values.ToArray() });

        return client.Name.Name;
    }

    public void UnregisterClient(string clientId)
    {
        if (!_connectedClients.Remove(clientId, out var client))
            return;

        if (!client.Name.IsCustom)
            _availableClientNames.Enqueue(client.Name.Name);

        ConnectedClientsChanged?.Invoke(this, new() { Clients = _connectedClients.Values.ToArray() });
    }

    public void SetClientActivity(string clientId, ClientActivity activity, string path)
    {
        if (!_connectedClients.TryGetValue(clientId, out var client))
            return;

        _connectedClients[clientId] = client with
        {
            CurrentActivity = activity,
            Path = path,
            LastUpdateTime = DateTimeOffset.UtcNow
        };

        ConnectedClientsChanged?.Invoke(this, new() { Clients = _connectedClients.Values.ToArray() });
    }

    public void SetClientName(string clientId, string name)
    {
        if (!_connectedClients.TryGetValue(clientId, out var client))
            return;

        if(!client.Name.IsCustom)
            _availableClientNames.Enqueue(client.Name.Name);

        _connectedClients[clientId] = client with
        {
            Name = new(name, true),
        };

        ConnectedClientsChanged?.Invoke(this, new() { Clients = _connectedClients.Values.ToArray() });
    }

    public IEnumerable<ConnectedClient> GetConnectedClients() => _connectedClients.Values;

    private ConnectedClient CreateNewClient(string clientId) =>
        new(clientId, GenerateUniqueName(), ClientActivity.Unknown, string.Empty, DateTimeOffset.UtcNow);

    private string GenerateUniqueName()
    {
        if (_availableClientNames.Count == 0)
            throw new NameListExhaustedException();

        return _availableClientNames.Dequeue();
    }
}

public sealed class NameListExhaustedException : Exception;

public record ConnectedClient(string Id, ClientName Name, ClientActivity CurrentActivity, string Path, DateTimeOffset LastUpdateTime);

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