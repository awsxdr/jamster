using System.Reflection;

namespace amethyst;

public static class RunningEnvironment
{
    public static readonly string RootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
};