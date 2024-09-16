using amethyst.DataStores;
using Func;

namespace amethyst.Services;

public interface IGameDiscoveryService
{
    IEnumerable<GameInfo> GetGames();
    GameInfo GetGame(GameInfo gameInfo);

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

    private GameInfo GetGameInfo(string gamePath)
    {
        using var gameStore = gameStoreFactory(Path.GetFileNameWithoutExtension(gamePath));

        var gameInfo = gameStore.GetInfo();

        return new(gameInfo.Id, gameInfo.Name);
    }
}