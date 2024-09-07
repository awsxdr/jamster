using System.Reflection;
using amethyst.DataStores;
using Func;

namespace amethyst.Services;

public interface IGameDiscoveryService
{
    IEnumerable<GameInfo> GetGames();
    GameInfo GetGame(GameInfo gameInfo);
}

public class GameDiscoveryService(GameStoreFactory gameStoreFactory, RunningEnvironment environment) : IGameDiscoveryService
{
    public const string GamesFolderName = "games";
    public string GamesFolder => Path.Combine(environment.RootPath, "db", GamesFolderName);

    public IEnumerable<GameInfo> GetGames() =>
        Directory.GetFiles(GamesFolder, "*.db")
            .Select(GetGameInfo);

    public GameInfo GetGame(GameInfo gameInfo)
    {
        using var gameStore = gameStoreFactory(GetGameFileName(gameInfo));
        gameStore.SetInfo(gameInfo);

        return gameStore.GetInfo();
    }

    private GameInfo GetGameInfo(string gamePath)
    {
        using var gameStore = gameStoreFactory(Path.GetFileNameWithoutExtension(gamePath));

        var gameInfo = gameStore.GetInfo();

        return new(gameInfo.Id, gameInfo.Name);
    }

    private static string GetGameFileName(GameInfo game) =>
        $"{CleanFileName(game.Name)}_{game.Id}";

    private static string CleanFileName(string value) =>
        value.Split(Path.GetInvalidFileNameChars())
            .Map(string.Concat)
            .Replace(" ", "")
            .Map(s => s[..Math.Min(20, s.Length)])
            .TrimEnd('.');
}