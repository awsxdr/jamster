using System.Reflection;

namespace jamster;

public static class RunningEnvironment
{
    public static bool IsDevelopment { get; internal set; }

    private static readonly Lazy<string> RootPathFactory = new(() =>
        IsDevelopment
        ? Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!
        : Directory.GetParent(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!)!.Parent!.FullName);

    private static string? _setRootPath;

    public static string RootPath
    {
        get => _setRootPath ?? RootPathFactory.Value;
        set => _setRootPath = value;
    }
};