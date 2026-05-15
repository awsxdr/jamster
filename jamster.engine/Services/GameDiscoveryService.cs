using jamster.engine.DataStores;
using jamster.engine.Domain;

namespace jamster.engine.Services;

public sealed class GamesListChangedEventArgs : EventArgs
{
    public required GameInfo[] Games { get; init; }
}

public interface IGameDiscoveryService
{
    event EventHandler<GamesListChangedEventArgs>? GamesListChanged;

    Task<GameInfo[]> GetGames();
    Task<GameInfo> GetGame(GameInfo gameInfo);
    Task<Result<GameInfo>> GetExistingGame(Guid gameId);
    bool GameExists(Guid gameId);
    Task<Result> ArchiveGame(Guid gameId);

    static string GetGameFileName(GameInfo game) =>
        $"{CleanFileName(game.Name)}_{game.Id}";

    private static string CleanFileName(string value) =>
        value.Split(Path.GetInvalidFileNameChars())
            .Map(string.Concat)
            .Replace(" ", "")
            .Map(s => s[..Math.Min(20, s.Length)])
            .TrimEnd('.');
}

[Singleton]
public class GameDiscoveryService : IGameDiscoveryService
{
    private readonly IGameDataStoreFactory _gameStoreFactory;

    public event EventHandler<GamesListChangedEventArgs>? GamesListChanged;

    public GameDiscoveryService(IGameDataStoreFactory gameStoreFactory)
    {
        _gameStoreFactory = gameStoreFactory;

        var fileSystemWatcher = new FileSystemWatcher(GameDataStore.GamesFolder, "*.db");

        fileSystemWatcher.Created += OnGameListChanged;
        fileSystemWatcher.Deleted += OnGameListChanged;
        fileSystemWatcher.Renamed += OnGameListChanged;
    }

    private void OnGameListChanged(object sender, FileSystemEventArgs e)
    {
        var games = GetGames().Result;
        GamesListChanged?.Invoke(this, new GamesListChangedEventArgs { Games = games });
    }

    public Task<GameInfo[]> GetGames() =>
        Task.WhenAll(
            Directory.GetFiles(GameDataStore.GamesFolder, "*.db")
                .Select(GetGameInfo));

    public async Task<GameInfo> GetGame(GameInfo gameInfo)
    {
        var gameStore = await _gameStoreFactory.GetDataStore(IGameDiscoveryService.GetGameFileName(gameInfo));
        gameStore.SetInfo(gameInfo);

        return gameStore.GetInfo();
    }

    public bool GameExists(Guid gameId) =>
        GetGameDatabasePathForId(gameId) is Success;

    public Task<Result<GameInfo>> GetExistingGame(Guid gameId) =>
        GetGameDatabasePathForId(gameId)
            .ThenMap(GetGameInfo);


    public Task<Result> ArchiveGame(Guid gameId) =>
        GetExistingGame(gameId)
            .ThenMap(IGameDiscoveryService.GetGameFileName)
            .Then(async gameFileName =>
            {
                using var @lock = await _gameStoreFactory.AcquireLock();
                await _gameStoreFactory.ReleaseConnection(gameFileName);

                var gameFileNameWithExtension = gameFileName + ".db";

                File.Move(Path.Combine(GameDataStore.GamesFolder, gameFileNameWithExtension), Path.Combine(GameDataStore.ArchiveFolder, gameFileNameWithExtension), overwrite: true);

                return Result.Succeed();
            });

    private static Result<string> GetGameDatabasePathForId(Guid gameId) =>
        Directory.GetFiles(GameDataStore.GamesFolder, $"*_{gameId}.db") switch
        {
            { Length: >1 } => Result<string>.Fail<MultipleGameFilesFoundForIdError>(),
            { Length: 0 } => Result<string>.Fail<GameFileNotFoundForIdError>(),
            [var file] => Result.Succeed(file)
        };

    private async Task<GameInfo> GetGameInfo(string gamePath)
    {
        var gameStore = await _gameStoreFactory.GetDataStore(Path.GetFileNameWithoutExtension(gamePath));

        var gameInfo = gameStore.GetInfo();

        return new(gameInfo.Id, gameInfo.Name);
    }

}

public sealed class MultipleGameFilesFoundForIdError : ResultError;
public sealed class GameFileNotFoundForIdError : NotFoundError;
