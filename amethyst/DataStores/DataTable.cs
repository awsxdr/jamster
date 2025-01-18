using System.Text.Json;
using System.Text.Json.Nodes;
using amethyst.Domain;
using Func;
using SQLite;

namespace amethyst.DataStores;

public delegate TKey KeySelector<in TData, out TKey>(TData dataItem) where TData : new();

public interface IDataTableFactory
{
    IDataTable<TData, TKey> Create<TData, TKey>(KeySelector<TData, TKey> keySelector, ISQLiteConnection connection) where TData : new();
}

public class DataTableFactory : IDataTableFactory
{
    public IDataTable<TData, TKey> Create<TData, TKey>(KeySelector<TData, TKey> keySelector, ISQLiteConnection connection)
        where TData : new() 
        =>
        new DataTable<TData, TKey>(keySelector, connection);
}

public interface IDataTable<TData, TKey> where TData : new()
{
    IEnumerable<TData> GetAll();
    IEnumerable<DataTable<TData, TKey>.DataTableItem> GetAllItems();
    Result<TData> Get(TKey key);
    Result<TData> GetIncludingArchived(TKey key);
    void Insert(TData item);
    void Upsert(TData item);
    Result Archive(TKey key);
    Result Update(TKey key, TData item);
    Result Update(TKey key, JsonObject item);
    Result Update(TKey key, string itemJson);
    bool Exists(TKey key);
}

public class DataTable<TData, TKey> : IDataTable<TData, TKey> where TData : new()
{
    private readonly KeySelector<TData, TKey> _keySelector;
    private readonly ISQLiteConnection _connection;
    private readonly string _tableName;

    public DataTable(KeySelector<TData, TKey> keySelector, ISQLiteConnection connection)
    {
        _keySelector = keySelector;
        _connection = connection;
        _tableName = typeof(TData).Name;

        var keyType = typeof(TKey).Name switch
        {
            "String" => "TEXT",
            "Int32" => "INTEGER",
            _ => "BLOB"
        };

        _connection.Execute($"CREATE TABLE IF NOT EXISTS {_tableName} (id {keyType} PRIMARY KEY, data TEXT, isArchived INTEGER)");
    }

    public IEnumerable<TData> GetAll() =>
        _connection.Query<DataTableItem>($"SELECT * FROM {_tableName} WHERE isArchived = FALSE", _tableName)
            .Select(MapItem);

    public IEnumerable<DataTableItem> GetAllItems() =>
        _connection.Query<DataTableItem>($"SELECT * FROM {_tableName}", _tableName);

    public Result<TData> Get(TKey key) =>
        _connection.Query<DataTableItem>($"SELECT id, data FROM {_tableName} WHERE id = ? AND isArchived = FALSE LIMIT 1", key)
            .Select(MapItem)
            .SingleOrDefault()
            ?.Map(x => Result.Succeed(x!))
        ?? Result<TData>.Fail<NotFoundError>();

    public Result<TData> GetIncludingArchived(TKey key) =>
        _connection.Query<DataTableItem>($"SELECT id, data FROM {_tableName} WHERE id = ? LIMIT 1", key)
            .Select(MapItem)
            .SingleOrDefault()
            ?.Map(x => Result.Succeed(x!))
        ?? Result<TData>.Fail<NotFoundError>();

    public void Insert(TData item) =>
        _connection.Execute($"INSERT INTO {_tableName} (id, data, isArchived) VALUES (?, ?, FALSE)", _keySelector(item), Serialize(item));

    public void Upsert(TData item)
    {
        var key = _keySelector(item);
        var data = Serialize(item);

        _connection.Execute(
            $"INSERT INTO {_tableName} (id, data, isArchived) VALUES (?, ?, FALSE) ON CONFLICT DO UPDATE SET data = ? WHERE id = ?",
            key,
            data,
            data,
            key);
    }

    public Result Archive(TKey key) =>
        _connection.Query<int>($"UPDATE {_tableName} SET isArchived = TRUE WHERE id = ? RETURNING 0", key).Count switch
        {
            1 => Result.Succeed(),
            0 => Result.Fail<TeamNotFoundError>(),
            _ => throw new UnexpectedUpdateCountException()
        };

    public Result Update(TKey key, TData item) => Update(key, Serialize(item));
    public Result Update(TKey key, JsonObject item) => Update(key, item.ToJsonString());

    public Result Update(TKey key, string itemJson) =>
        _connection.Query<int>($"UPDATE {_tableName} SET data = ? WHERE isArchived = FALSE AND id = ? RETURNING 0", itemJson, key).Count switch
        {
            1 => Result.Succeed(),
            0 => Result.Fail<TeamNotFoundError>(),
            _ => throw new UnexpectedUpdateCountException()
        };

    public bool Exists(TKey key) =>
        _connection.ExecuteScalar<int>($"SELECT COUNT(id) FROM {_tableName} WHERE isArchived = FALSE AND id = ? LIMIT 1", key) == 1;

    private static TData MapItem(DataTableItem item) =>
        JsonSerializer.Deserialize<TData>(item.Data) ?? new();

    private static string Serialize(TData data) => JsonSerializer.Serialize(data);

    public record DataTableItem(TKey Id, string Data, bool IsArchived)
    {
        public DataTableItem() : this(default!, string.Empty, false)
        {
        }
    }
}