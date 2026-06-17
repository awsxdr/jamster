using jamster.engine.Domain;

namespace jamster.engine.Services;

public interface ICustomScreensService
{
    Task<CustomScreen[]> GetCustomScreens();
    Task<Result<CustomScreenPath>> GetScreenFileLocation(Guid screenId, string subPath);
}

[Singleton]
public class CustomScreensService(IEnumerable<ICustomScreenDiscoveryService> customScreenDiscoveryServices) : ICustomScreensService
{
    public Task<CustomScreen[]> GetCustomScreens() =>
        customScreenDiscoveryServices.SelectMany(c => c.GetCustomScreens()).ToArray().ToTask();

    public Task<Result<CustomScreenPath>> GetScreenFileLocation(Guid screenId, string subPath)
    {
        var screens = customScreenDiscoveryServices.SelectMany(c => c.GetCustomScreens()).ToArray();

        var targetScreen = screens.SingleOrDefault(s => s.Id == screenId);

        if (targetScreen == null)
            return Result<CustomScreenPath>.Fail<ScreenNotFoundError>().ToTask();

        return targetScreen.Path switch
        {
            CustomScreenUrlPath => Result.Succeed(targetScreen.Path).ToTask(),
            CustomScreenFilePath relativePath => ConvertToAbsoluteFilePath(relativePath, subPath).ToTask(),
            _ => throw new PathTypeNotSupportedException()
        };
    }

    private static Result<CustomScreenPath> ConvertToAbsoluteFilePath(CustomScreenFilePath filePath, string subPath)
    {
        var absolutePath = Path.GetFullPath(Path.Combine(filePath.FilePath, subPath));

        if (!absolutePath.StartsWith(filePath.FilePath))
            return Result<CustomScreenPath>.Fail<PathTraversalDetectedError>();

        if (!File.Exists(absolutePath))
            return Result<CustomScreenPath>.Fail<FileNotFoundError>();

        return Result.Succeed<CustomScreenPath>(new CustomScreenFilePath(absolutePath));
    }

    public sealed class ScreenNotFoundError : NotFoundError;
    public sealed class FileNotFoundError : NotFoundError;
    public sealed class PathTraversalDetectedError : ResultError;

    public sealed class RedirectRequiredError(string redirectUrl) : ResultError
    {
        public string RedirectUrl => redirectUrl;
    }

    public sealed class PathTypeNotSupportedException : Exception;
}
