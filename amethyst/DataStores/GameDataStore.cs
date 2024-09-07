using System.Text.Json;
using amethyst.Services;

namespace amethyst.DataStores;

public delegate GameDataStore GameStoreFactory(string databasePath);

public interface IGameDataStore : IEventStore
{
    GameInfo GetInfo();
    void SetInfo(GameInfo info);
}

public class GameDataStore : EventStore, IGameDataStore
{
    public GameDataStore(string databasePath, ConnectionFactory connectionFactory, RunningEnvironment environment) 
        : base(Path.Combine(GameDiscoveryService.GamesFolderName, databasePath), connectionFactory, environment)
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
}

public record GameInfo(Guid Id, string Name)
{
    public GameInfo() : this(Guid.NewGuid(), string.Empty)
    {
    }
}
