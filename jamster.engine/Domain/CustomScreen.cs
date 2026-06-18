namespace jamster.engine.Domain;

public record CustomScreen(
    Guid Id,
    string Name,
    string Category,
    bool OwnTab,
    CustomScreenPath Path,
    CustomScreenType Type);

public abstract record CustomScreenPath;
public record CustomScreenUrlPath(string Url) : CustomScreenPath;
public record CustomScreenFilePath(string FilePath) : CustomScreenPath;

public enum CustomScreenType
{
    Jamster,
    Carolina,
}
