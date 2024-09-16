using SQLite;

namespace amethyst.DataStores;

public interface IEventStore : IDisposable
{
}

public abstract class EventStore : IEventStore
{
    public ISQLiteConnection Connection { get; }

    protected EventStore(string databaseName, ConnectionFactory connectionFactory)
    {
        Connection = connectionFactory(Path.Combine(RunningEnvironment.RootPath, "db", $"{databaseName}.db"), SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
    }

    public void Dispose()
    {
        Connection.Dispose();
    }
}