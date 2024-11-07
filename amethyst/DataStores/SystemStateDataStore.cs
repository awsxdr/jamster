using amethyst.Domain;
using Func;

namespace amethyst.DataStores;

public interface ISystemStateDataStore : IDisposable
{
    Result<Guid> GetCurrentGame();
    void SetCurrentGame(Guid gameId);
}

public class SystemStateDataStore(ConnectionFactory connectionFactory)
    : DataStore<SystemStateItem, string>("system", 1, i => i.Key, connectionFactory), ISystemStateDataStore
{
    private const string CurrentGameKey = "current_game";

    public Result<Guid> GetCurrentGame() =>
        Get(CurrentGameKey) switch
        {
            Success<SystemStateItem> s when Guid.TryParse(s.Value.Value, out var id) => Result.Succeed(id),
            _ => Result<Guid>.Fail<CurrentGameNotFoundError>()
        };

    public void SetCurrentGame(Guid gameId) =>
        Upsert(new(CurrentGameKey, gameId.ToString()));

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