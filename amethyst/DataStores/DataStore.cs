using System.Text.Json;
using amethyst.Domain;
using Func;
using SQLite;

using static Func.Result;

namespace amethyst.DataStores;

public interface IDataStore : IDisposable
{
    internal ISQLiteConnection Connection { get; }
}

public delegate ISQLiteConnection ConnectionFactory(string connectionString, SQLiteOpenFlags flags);

public abstract class DataStore<TData, TKey> : IDataStore
    where TData : new()
{
    private readonly KeySelector _keySelector;
    private readonly string _tableName;

    protected delegate TKey KeySelector(TData dataItem);

    public ISQLiteConnection Connection { get; }

    protected DataStore(string databaseName, KeySelector keySelector, ConnectionFactory connectionFactory)
    {
        _keySelector = keySelector;
        _tableName = typeof(TData).Name;
        Connection = connectionFactory(Path.Combine(RunningEnvironment.RootPath, "db", $"{databaseName}.db"), SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite);

        Connection.Execute($"CREATE TABLE IF NOT EXISTS {_tableName} (id BLOB PRIMARY KEY, data TEXT, isArchived INTEGER)");
    }

    protected IEnumerable<TData> GetAll() =>
        Connection.Query<DataStoreItem>($"SELECT * FROM {_tableName} WHERE isArchived = FALSE", _tableName)
            .Select(MapItem);

    protected Result<TData> Get(TKey key) =>
        Connection.Query<DataStoreItem>($"SELECT id, data FROM {_tableName} WHERE id = ? AND isArchived = FALSE LIMIT 1", key)
            .Select(MapItem)
            .SingleOrDefault()
            ?.Map(x => Succeed(x!))
        ?? Result<TData>.Fail<NotFoundError>();

    protected Result<TData> GetIncludingArchived(TKey key) =>
        Connection.Query<DataStoreItem>($"SELECT id, data FROM {_tableName} WHERE id = ? LIMIT 1", key)
            .Select(MapItem)
            .SingleOrDefault()
            ?.Map(x => Succeed(x!))
        ?? Result<TData>.Fail<NotFoundError>();

    protected void Insert(TData item) =>
        Connection.Execute($"INSERT INTO {_tableName} (id, data, isArchived) VALUES (?, ?, FALSE)", _keySelector(item), Serialize(item));

    protected void Upsert(TData item)
    {
        var key = _keySelector(item);
        var data = Serialize(item);

        Connection.Execute(
            $"INSERT INTO {_tableName} (id, data, isArchived) VALUES (?, ?, FALSE) ON CONFLICT DO UPDATE SET data = ? WHERE id = ?",
            key,
            data,
            data,
            key);
    }

    protected Result Archive(TKey key) =>
        Connection.Query<int>($"UPDATE {_tableName} SET isArchived = TRUE WHERE id = ? RETURNING 0", key).Count switch
        {
            1 => Succeed(),
            0 => Fail<NotFoundError>(),
            _ => throw new UnexpectedUpdateCountException()
        };

    protected void Update(TKey key, TData item) =>
        Connection.Execute($"UPDATE {_tableName} SET data = ? WHERE id = ?", Serialize(item), key);

    protected bool Exists(TKey key) =>
        Connection.ExecuteScalar<int>($"SELECT COUNT(id) FROM {_tableName} WHERE isArchived = FALSE AND id = ? LIMIT 1", key) == 1;

    private static TData MapItem(DataStoreItem item) =>
        JsonSerializer.Deserialize<TData>(item.Data) ?? new();

    private static string Serialize(TData data) => JsonSerializer.Serialize(data);

    public void Dispose()
    {
        Connection.Dispose();
    }

    internal record DataStoreItem(TKey Id, string Data, bool IsArchived)
    {
        public DataStoreItem() : this(default, string.Empty, false)
        {
        }
    }
}

public class UnexpectedUpdateCountException : Exception;