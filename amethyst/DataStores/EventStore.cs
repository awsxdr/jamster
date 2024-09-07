using SQLite;

namespace amethyst.DataStores;

public interface IEventStore : IDisposable
{
}

public abstract class EventStore : IEventStore
{
    public ISQLiteConnection Connection { get; }

    protected EventStore(string databaseName, ConnectionFactory connectionFactory, RunningEnvironment environment)
    {
        Connection = connectionFactory(Path.Combine(environment.RootPath, "db", $"{databaseName}.db"), SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
    }

    public void Dispose()
    {
        Connection.Dispose();
    }
}