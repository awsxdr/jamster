using SQLite;

namespace jamster.engine.DataStores;

public interface IEventStore : IDisposable
{
    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction();
}

public abstract class EventStore : IEventStore
{
    public ISQLiteConnection Connection { get; }
    protected string DatabaseName { get; }

    protected EventStore(string databaseName, ConnectionFactory connectionFactory)
    {
        Connection = connectionFactory(Path.Combine(RunningEnvironment.RootPath, "db", $"{databaseName}.db"), SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);
        DatabaseName = databaseName;
    }

    public void BeginTransaction() => Connection.BeginTransaction();

    public void CommitTransaction() => Connection.Commit();

    public void RollbackTransaction() => Connection.Rollback();

    public void Dispose()
    {
        Connection.Close();
        Connection.Dispose();
    }
}