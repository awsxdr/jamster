using System.Text.Json;
using amethyst.Services;

namespace amethyst.DataStores;

using Events;
using Reducers;

public delegate IGameDataStore GameStoreFactory(string databaseName);

public interface IGameDataStore : IEventStore
{
    GameInfo GetInfo();
    void SetInfo(GameInfo info);
    Guid AddEvent(Event @event);
    IEnumerable<Event> GetEvents();
}

public class GameDataStore : EventStore, IGameDataStore
{
    public const string GamesFolderName = "games";
    public static string GamesFolder => Path.Combine(RunningEnvironment.RootPath, "db", GamesFolderName);

    public GameDataStore(string databaseName, ConnectionFactory connectionFactory) 
        : base(Path.Combine(GamesFolderName, databaseName), connectionFactory)
    {
        Connection.Execute("CREATE TABLE IF NOT EXISTS gameInfo (id SMALLINT PRIMARY KEY, info TEXT)");
        Connection.Execute("INSERT INTO gameInfo (id, info) VALUES (0, ?) ON CONFLICT DO NOTHING", JsonSerializer.Serialize(new GameInfo()));
    }

    public GameInfo GetInfo() =>
        JsonSerializer.Deserialize<GameInfo>(
            Connection.ExecuteScalar<string>("SELECT info FROM gameInfo LIMIT 1")
        )!;

    public void SetInfo(GameInfo info) =>
        Connection.Execute("UPDATE gameInfo SET info = ?", JsonSerializer.Serialize(info));

    public Guid AddEvent(Event @event)
    {
        var eventId = Guid.NewGuid(); //TODO: Change to GUIDv7
        //TODO: Add event to database
        return eventId;
    }

    public IEnumerable<Event> GetEvents()
    {
        return [];
    }
}

public record GameInfo(Guid Id, string Name)
{
    public GameInfo() : this(Guid.NewGuid(), string.Empty)
    {
    }
}
