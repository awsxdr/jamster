using System.Reflection;

namespace amethyst;

public static class RunningEnvironment
{
    public static string RootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
};