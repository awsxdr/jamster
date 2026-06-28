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

public interface IFileSystemWatcher : IDisposable
{
    event FileSystemEventHandler? Created;
    event FileSystemEventHandler? Deleted;
    event RenamedEventHandler? Renamed;

    bool EnableRaisingEvents { get; set; }
}

public class FileSystemWatcherWrapper(string path, string filter = "*.*") : IFileSystemWatcher
{
    public delegate IFileSystemWatcher Factory(string path, string filter);

    private readonly FileSystemWatcher _fileSystemWatcher = new(path, filter);

    public event FileSystemEventHandler? Created
    {
        add => _fileSystemWatcher.Created += value;
        remove => _fileSystemWatcher.Created -= value;
    }

    public event FileSystemEventHandler? Deleted
    {
        add => _fileSystemWatcher.Deleted += value;
        remove => _fileSystemWatcher.Deleted -= value;
    }

    public event RenamedEventHandler? Renamed
    {
        add => _fileSystemWatcher.Renamed += value;
        remove => _fileSystemWatcher.Renamed -= value;
    }
    public bool EnableRaisingEvents
    {
        get => _fileSystemWatcher.EnableRaisingEvents;
        set => _fileSystemWatcher.EnableRaisingEvents = value;
    }

    public void Dispose() => _fileSystemWatcher.Dispose();
}

[Singleton]
public class GameDiscoveryService : IGameDiscoveryService, IDisposable
{
    private readonly IGameDataStoreFactory _gameStoreFactory;
    private readonly ILogger<GameDiscoveryService> _logger;
    private readonly IFileSystemWatcher _fileSystemWatcher;

    public event EventHandler<GamesListChangedEventArgs>? GamesListChanged;

    public GameDiscoveryService(
        IGameDataStoreFactory gameStoreFactory, 
        FileSystemWatcherWrapper.Factory fileSystemWatcherFactory,
        ILogger<GameDiscoveryService> logger
    )
    {
        _gameStoreFactory = gameStoreFactory;
        _logger = logger;

        _fileSystemWatcher = fileSystemWatcherFactory(GameDataStore.GamesFolder, "*.db");
        _fileSystemWatcher.EnableRaisingEvents = true;

        _fileSystemWatcher.Created += OnGameListChanged;
        _fileSystemWatcher.Deleted += OnGameListChanged;
        _fileSystemWatcher.Renamed += OnGameListChanged;
    }

    private async void OnGameListChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            var games = await GetGames();
            GamesListChanged?.Invoke(this, new GamesListChangedEventArgs { Games = games });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating game list following file change");
        }
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

    public void Dispose() => _fileSystemWatcher.Dispose();
}

public sealed class MultipleGameFilesFoundForIdError : ResultError;
public sealed class GameFileNotFoundForIdError : NotFoundError;
