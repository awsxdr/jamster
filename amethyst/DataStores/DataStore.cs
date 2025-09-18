using SQLite;

namespace amethyst.DataStores;

public interface IDataStore : IDisposable
{
    public ISQLiteConnection Connection { get; }
    void BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction();
}

public delegate ISQLiteConnection ConnectionFactory(string connectionString, SQLiteOpenFlags flags);

public abstract class DataStore : IDataStore
{
    private readonly IDataTableFactory _dataTableFactory;
    public ISQLiteConnection Connection { get; }

    protected DataStore(string databaseName, int version, ConnectionFactory connectionFactory, IDataTableFactory dataTableFactory)
    {
        _dataTableFactory = dataTableFactory;
        Connection = connectionFactory(Path.Combine(RunningEnvironment.RootPath, "db", $"{databaseName}.db"), SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);

        Connection.Execute("CREATE TABLE IF NOT EXISTS __version (version INTEGER PRIMARY KEY)");
        var currentVersion = Connection.QueryScalars<int>("SELECT version FROM __version LIMIT 1").SingleOrDefault();

        if (currentVersion > version)
        {
            throw new DatabaseVersionAheadOfProgramVersion();
        }

        if (currentVersion < version)
        {
            for (var i = currentVersion + 1; i <= version; ++i)
            {
                ApplyUpgrade(i);
            }

            Connection.Execute("DELETE FROM __version");
            Connection.Execute("INSERT INTO __version (version) VALUES (?)", version);
        }
    }

    protected abstract void ApplyUpgrade(int version);

    protected IDataTable<TData, TKey> GetTable<TData, TKey>(KeySelector<TData, TKey> keySelector) where TData : new() =>
        _dataTableFactory.Create(keySelector, Connection, []);

    protected IDataTable<TData, TKey> GetTable<TData, TKey>(KeySelector<TData, TKey> keySelector, IEnumerable<IColumn> columns) where TData : new() =>
        _dataTableFactory.Create(keySelector, Connection, columns);

    public void BeginTransaction() =>
        Connection.BeginTransaction();

    public void CommitTransaction() =>
        Connection.Commit();

    public void RollbackTransaction() =>
        Connection.Rollback();

    public void Dispose() =>
        Connection.Dispose();
}

public class UnexpectedUpdateCountException : Exception;
public class DatabaseVersionAheadOfProgramVersion : Exception;