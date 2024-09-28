using amethyst.DataStores;
using Func;

namespace amethyst.Services;

using Domain;

public interface IGameDiscoveryService
{
    IEnumerable<GameInfo> GetGames();
    GameInfo GetGame(GameInfo gameInfo);
    Result<GameInfo> GetExistingGame(Guid gameId);
    bool GameExists(Guid gameId);

    public static string GetGameFileName(GameInfo game) =>
        $"{CleanFileName(game.Name)}_{game.Id}";

    private static string CleanFileName(string value) =>
        value.Split(Path.GetInvalidFileNameChars())
            .Map(string.Concat)
            .Replace(" ", "")
            .Map(s => s[..Math.Min(20, s.Length)])
            .TrimEnd('.');
}

public class GameDiscoveryService(GameStoreFactory gameStoreFactory) : IGameDiscoveryService
{
    public IEnumerable<GameInfo> GetGames() =>
        Directory.GetFiles(GameDataStore.GamesFolder, "*.db")
            .Select(GetGameInfo);

    public GameInfo GetGame(GameInfo gameInfo)
    {
        using var gameStore = gameStoreFactory(IGameDiscoveryService.GetGameFileName(gameInfo));
        gameStore.SetInfo(gameInfo);

        return gameStore.GetInfo();
    }

    public bool GameExists(Guid gameId) =>
        GetGameNameForId(gameId) is Success;

    public Result<GameInfo> GetExistingGame(Guid gameId) =>
        GetGameNameForId(gameId)
            .ThenMap(GetGameInfo);

    private static Result<string> GetGameNameForId(Guid gameId) =>
        Directory.GetFiles(GameDataStore.GamesFolder, $"*_{gameId}.db") switch
        {
            { Length: >1 } => Result<string>.Fail<MultipleGameFilesFoundForIdError>(),
            { Length: 0 } => Result<string>.Fail<GameFileNotFoundForIdError>(),
            [var file] => Result.Succeed(file)
        };

    private GameInfo GetGameInfo(string gamePath)
    {
        using var gameStore = gameStoreFactory(Path.GetFileNameWithoutExtension(gamePath));

        var gameInfo = gameStore.GetInfo();

        return new(gameInfo.Id, gameInfo.Name);
    }

}

public sealed class MultipleGameFilesFoundForIdError : ResultError;
public sealed class GameFileNotFoundForIdError : NotFoundError;
