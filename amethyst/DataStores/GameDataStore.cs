using System.Text.Json;
using System.Text.Json.Nodes;
using amethyst.Services;
using Func;

namespace amethyst.DataStores;

using Events;

public interface IGameDataStore : IEventStore
{
    GameInfo GetInfo();
    void SetInfo(GameInfo info);
    Guid AddEvent(Event @event);
    IEnumerable<Event> GetEvents();
    Result<Event> GetEvent(Guid id);
    void DeleteEvent(Guid eventId);
}

public class GameDataStore : EventStore, IGameDataStore
{
    private readonly IEventConverter _eventConverter;
    private readonly ILogger<GameDataStore> _logger;

    public delegate IGameDataStore Factory(string databaseName);

    public const string GamesFolderName = "games";
    public const string ArchiveFolderName = "archive";

    public static string GamesFolder => Path.Combine(RunningEnvironment.RootPath, "db", GamesFolderName);
    public static string ArchiveFolder => Path.Combine(RunningEnvironment.RootPath, "db", GamesFolderName, ArchiveFolderName);

    public GameDataStore(
        string databaseName,
        ConnectionFactory connectionFactory, 
        IEventConverter eventConverter, 
        ILogger<GameDataStore> logger) 
        : base(Path.Combine(GamesFolderName, databaseName), connectionFactory)
    {
        _eventConverter = eventConverter;
        _logger = logger;
        Connection.Execute("CREATE TABLE IF NOT EXISTS gameInfo (id SMALLINT PRIMARY KEY, info TEXT)");
        Connection.Execute("INSERT INTO gameInfo (id, info) VALUES (0, ?) ON CONFLICT DO NOTHING", JsonSerializer.Serialize(new GameInfo(), Program.JsonSerializerOptions));

        Connection.Execute("CREATE TABLE IF NOT EXISTS events (id BLOB PRIMARY KEY, type TEXT, body TEXT)");
    }

    public GameInfo GetInfo() =>
        JsonSerializer.Deserialize<GameInfo>(
            Connection.ExecuteScalar<string>("SELECT info FROM gameInfo LIMIT 1"),
            Program.JsonSerializerOptions
        )!;

    public void SetInfo(GameInfo info) =>
        Connection.Execute("UPDATE gameInfo SET info = ?", JsonSerializer.Serialize(info, Program.JsonSerializerOptions));

    public Guid AddEvent(Event @event)
    {
        var eventId = @event.Id == Guid.Empty ? Guid7.NewGuid() : @event.Id;

        Connection.Execute(
            "INSERT INTO events (id, type, body) VALUES (?, ?, ?)", 
            (Guid) eventId,
            @event.Type,
            @event.HasBody ? JsonSerializer.Serialize(@event.GetBodyObject(), Program.JsonSerializerOptions) : null);

        return eventId;
    }

    public IEnumerable<Event> GetEvents()
    {
        _logger.LogDebug("Loading events from {databaseName}", DatabaseName);

        var events = Connection.Query<EventItem>("SELECT * FROM events ORDER BY id");

        var eventDecodeResults =
            events
                .Select(EventItemToUntypedEvent)
                .Select(_eventConverter.DecodeEvent)
                .ToArray();

        var failedDecodeCount = eventDecodeResults.Count(r => r is Failure);

        if(failedDecodeCount > 0)
            _logger.LogError("Failed to decode {failedDecodeCount} events from database {databaseName}", failedDecodeCount, DatabaseName);

        return
            eventDecodeResults
                .OfType<Success<Event>>()
                .Select(s => s.Value)
                .ToArray();
    }

    public Result<Event> GetEvent(Guid id)
    {
        var events = Connection.Query<EventItem>("SELECT * FROM events WHERE id = ?", id);

        return
            events.Count == 1
                ? _eventConverter.DecodeEvent(EventItemToUntypedEvent(events[0]))
                : Result<Event>.Fail<EventNotFoundError>();
    }

    public void DeleteEvent(Guid eventId)
    {
        _logger.LogDebug("Deleting event {eventId}", eventId);

        Connection.Execute("DELETE FROM events WHERE id = ?", eventId);
    }

    private static IUntypedEvent EventItemToUntypedEvent(EventItem @event) =>
        string.IsNullOrWhiteSpace(@event.Body)
            ? new UntypedEvent(@event.Type, @event.Id)
            : new UntypedEventWithBody(@event.Type, @event.Id, JsonNode.Parse(@event.Body)!.AsObject());

    public sealed class EventNotFoundError : ResultError;
}

public record GameInfo(Guid Id, string Name)
{
    public GameInfo() : this(Guid.NewGuid(), string.Empty)
    {
    }
}

public record EventItem(Guid Id, string Type, string? Body)
{
    public EventItem() : this(Guid7.NewGuid(), string.Empty, string.Empty)
    {
    }
}