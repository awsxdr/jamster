using jamster.engine.Domain;

namespace jamster.engine.DataStores;

public interface ISystemStateDataStore : IDisposable
{
    Result<Guid> GetCurrentGame();
    Result SetCurrentGame(Guid gameId);
}

public class SystemStateDataStore : DataStore, ISystemStateDataStore
{
    private const string CurrentGameKey = "current_game";

    private readonly IDataTable<SystemStateItem, string> _systemStateTable;

    public SystemStateDataStore(ConnectionFactory connectionFactory, IDataTableFactory dataTableFactory)
        : base("system", 1, connectionFactory, dataTableFactory)
    {
        _systemStateTable = GetTable<SystemStateItem, string>(s => s.Key);
    }

    public Result<Guid> GetCurrentGame() =>
        _systemStateTable.Get(CurrentGameKey) switch
        {
            Success<SystemStateItem> s when Guid.TryParse(s.Value.Value, out var id) => Result.Succeed(id),
            _ => Result<Guid>.Fail<CurrentGameNotFoundError>()
        };

    public Result SetCurrentGame(Guid gameId) =>
        _systemStateTable.Upsert(new(CurrentGameKey, gameId.ToString()));

    protected override void ApplyUpgrade(int version)
    {
    }

    public sealed class CurrentGameNotFoundError : NotFoundError;
}

public sealed record SystemStateItem(string Key, string Value)
{
    public SystemStateItem() : this("", "")
    {
    }
}