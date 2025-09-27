using System.Text.Json;
using System.Text.Json.Nodes;

using jamster.engine.Domain;

using SQLite;

namespace jamster.engine.DataStores;

public delegate TKey KeySelector<in TData, out TKey>(TData dataItem) where TData : new();

public interface IDataTableFactory
{
    IDataTable<TData, TKey> Create<TData, TKey>(KeySelector<TData, TKey> keySelector, ISQLiteConnection connection, IEnumerable<IColumn> columns) where TData : new();
}

public class DataTableFactory : IDataTableFactory
{
    public IDataTable<TData, TKey> Create<TData, TKey>(KeySelector<TData, TKey> keySelector, ISQLiteConnection connection, IEnumerable<IColumn> columns)
        where TData : new()
    {
        var table = new DataTable<TData, TKey>(keySelector, connection, columns.ToArray());
        table.CreateTablesIfRequired();
        return table;
    }
}

public interface IDataTable;

public interface IDataTable<TData, TKey> : IDataTable where TData : new()
{
    IReadOnlyDictionary<string, IColumn> Columns { get; }
    KeySelector<TData, TKey> KeySelector { get; }

    IEnumerable<TData> GetAll();
    IEnumerable<DataTable<TData, TKey>.DataTableItem> GetAllItems();
    Result<TData> Get(TKey key);
    IEnumerable<TData> GetByColumn(IColumn column, object key);
    Result<TData> GetIncludingArchived(TKey key);
    IEnumerable<TData> GetByColumnIncludingArchived(IColumn column, object key);
    bool Insert(TData item);
    Result Upsert(TData item);
    Result Archive(TKey key);
    Result ArchiveByColumn(IColumn column, object key);
    Result Update(TKey key, TData item);
    Result Update(TKey key, JsonObject item);
    Result Update(TKey key, string itemJson);
    bool Exists(TKey key);
}

public class DataTable<TData, TKey>(KeySelector<TData, TKey> keySelector, ISQLiteConnection connection, IColumn[] columns) : IDataTable<TData, TKey>
    where TData : new()
{
    private readonly string _tableName = typeof(TData).Name;
    private readonly string _columnHeaders = columns.Select(c => $"{c.Name}, ").Map(string.Concat);

    public IReadOnlyDictionary<string, IColumn> Columns { get; } = columns.ToDictionary(x => x.Name, x => x);
    public KeySelector<TData, TKey> KeySelector => keySelector;

    public IEnumerable<TData> GetAll() =>
        connection.Query<DataTableItem>($"SELECT * FROM {_tableName} WHERE isArchived = FALSE", _tableName)
            .Select(MapItem);

    public IEnumerable<DataTableItem> GetAllItems() =>
        connection.Query<DataTableItem>($"SELECT * FROM {_tableName}", _tableName);

    public Result<TData> Get(TKey key) =>
        connection.Query<DataTableItem>($"SELECT id, data FROM {_tableName} WHERE id = ? AND isArchived = FALSE LIMIT 1", key)
            .Select(MapItem)
            .SingleOrDefault()
            ?.Map(x => Result.Succeed(x!))
        ?? Result<TData>.Fail<NotFoundError>();

    public IEnumerable<TData> GetByColumn(IColumn column, object key)
    {
        if (!key.GetType().IsAssignableTo(column.ColumnType))
            throw new FilterTypeDoesNotMatchColumnTypeException();

        var query = $"SELECT id, data from {_tableName} WHERE {column.Name} = ? AND isArchived = FALSE";

        return connection.Query<DataTableItem>(query, key)
            .Select(MapItem)
            .ToArray();
    }

    public Result<TData> GetIncludingArchived(TKey key) =>
        connection.Query<DataTableItem>($"SELECT id, data FROM {_tableName} WHERE id = ? LIMIT 1", key)
            .Select(MapItem)
            .SingleOrDefault()
            ?.Map(x => Result.Succeed(x!))
        ?? Result<TData>.Fail<NotFoundError>();

    public IEnumerable<TData> GetByColumnIncludingArchived(IColumn column, object key)
    {
        if (!key.GetType().IsAssignableTo(column.ColumnType))
            throw new FilterTypeDoesNotMatchColumnTypeException();

        var query = $"SELECT id, data from {_tableName} WHERE {column.Name} = ?";

        return connection.Query<DataTableItem>(query, key)
            .Select(MapItem)
            .ToArray();
    }

    public bool Insert(TData item)
    {
        var query = $"INSERT INTO {_tableName} (id, data, {_columnHeaders}isArchived) VALUES (?, ?, {columns.Select(_ => "?, ").Map(string.Concat)}FALSE) ON CONFLICT DO NOTHING";

        var parameters = new object?[] { KeySelector(item), Serialize(item) }
            .Concat(columns.Select(c => c.GetValue(item!)))
            .ToArray();

        return connection.Execute(query, parameters) > 0;
    }

    public Result Upsert(TData item)
    {
        var query = $"INSERT INTO {_tableName} (id, data, {_columnHeaders}isArchived) VALUES (?, ?, {columns.Select(_ => "?, ").Map(string.Concat)}FALSE) ON CONFLICT DO UPDATE SET data = ? WHERE id = ?";
        var key = KeySelector(item);
        var data = Serialize(item);

        var parameters = new object?[] { key, data }
            .Concat(columns.Select(c => c.GetValue(item!)))
            .Append(data)
            .Append(key)
            .ToArray();

        return connection.Execute(query, parameters) switch
        {
            1 => Result.Succeed(),
            0 => throw new UnableToUpsertException(),
            _ => throw new UnexpectedUpdateCountException()
        };
    }

    public Result Archive(TKey key) =>
        connection.Query<int>($"UPDATE {_tableName} SET isArchived = TRUE WHERE id = ? RETURNING 0", key).Count switch
        {
            1 => Result.Succeed(),
            0 => Result.Fail<NotFoundError>(),
            _ => throw new UnexpectedUpdateCountException()
        };

    public Result ArchiveByColumn(IColumn column, object key) =>
        connection.Query<int>($"UPDATE {_tableName} SET isArchived = TRUE WHERE {column.Name} = ? RETURNING 0", key).Count switch
        {
            0 => Result.Fail<NotFoundError>(),
            _ => Result.Succeed()
        };

    public Result Update(TKey key, TData item) => Update(key, Serialize(item));
    public Result Update(TKey key, JsonObject item) => Update(key, item.ToJsonString());

    public Result Update(TKey key, string itemJson) =>
        connection.Query<int>($"UPDATE {_tableName} SET data = ? WHERE isArchived = FALSE AND id = ? RETURNING 0", itemJson, key).Count switch
        {
            1 => Result.Succeed(),
            0 => Result.Fail<NotFoundError>(),
            _ => throw new UnexpectedUpdateCountException()
        };

    public bool Exists(TKey key) =>
        connection.ExecuteScalar<int>($"SELECT COUNT(id) FROM {_tableName} WHERE isArchived = FALSE AND id = ? LIMIT 1", key) == 1;

    private static TData MapItem(DataTableItem item) =>
        JsonSerializer.Deserialize<TData>(item.Data, Program.JsonSerializerOptions) ?? new();

    private static string Serialize(TData data) => JsonSerializer.Serialize(data, Program.JsonSerializerOptions);

    public record DataTableItem(TKey Id, string Data, bool IsArchived)
    {
        public DataTableItem() : this(default!, string.Empty, false)
        {
        }
    }

    public void CreateTablesIfRequired()
    {
        var keyType = GetSqliteType(typeof(TKey));

        var columnString = columns.Select(c => $"{c.Name} {GetSqliteType(c.ColumnType)}, ").Map(string.Concat);
        connection.Execute($"CREATE TABLE IF NOT EXISTS {_tableName} (id {keyType} PRIMARY KEY, {columnString} data TEXT, isArchived INTEGER)");

        return;

        string GetSqliteType(Type type) => type.Name switch
        {
            "String" => "TEXT",
            "Int32" => "INTEGER",
            _ => "BLOB"
        };
    }
}

public sealed class FilterTypeDoesNotMatchColumnTypeException : Exception;
public sealed class UnableToUpsertException : Exception;

public interface IColumn
{
    Type ColumnType { get; }
    string Name { get; }
    object? GetValue(object dataItem);
}

public class Column<TColumn, TData>(string name, KeySelector<TData, TColumn> keySelector) : IColumn where TData : new()
{
    public Type ColumnType { get; } = typeof(TColumn);
    public string Name { get; } = name;
    object? IColumn.GetValue(object dataItem) => dataItem is TData d ? GetValue(d) : null;
    public TColumn GetValue(TData dataItem) => keySelector(dataItem);
}