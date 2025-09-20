using System.Reflection;

namespace jamster;

public static class RunningEnvironment
{
    public static bool IsDevelopment { get; internal set; }

    private static readonly Lazy<string> RootPathFactory = new(() =>
        Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!);

    private static string? _setRootPath;

    public static string RootPath
    {
        get => _setRootPath ?? RootPathFactory.Value;
        set => _setRootPath = value;
    }
};