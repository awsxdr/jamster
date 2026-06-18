using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using jamster.engine.Domain;

namespace jamster.engine.Services;

public interface ICustomScreenDiscoveryService
{
    CustomScreen[] GetCustomScreens();
}

[Singleton, RegisterOnOption(nameof(CommandLineOptions.EnableCarolinaCompatibility))]
public partial class CarolinaCustomScreenDiscoveryService : ICustomScreenDiscoveryService
{
    private readonly string _screensPath = Path.Combine(RunningEnvironment.RootPath, "custom-screens", "crg-compatible");

    [GeneratedRegex("<head>.*?<title>(?<title>.*?)</title>.*?</head>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TitleRegex();

    public CarolinaCustomScreenDiscoveryService()
    {
        Directory.CreateDirectory(_screensPath);
    }

    public CustomScreen[] GetCustomScreens() =>
        Directory.GetDirectories(_screensPath)
            .Where(d => File.Exists(Path.Combine(d, "index.html")) || File.Exists(Path.Combine(d, "index.htm")))
            .Select(ReadCustomScreen)
            .ToArray();

    private static CustomScreen ReadCustomScreen(string path) =>
        new(
            GetScreenId(path),
            GetScreenName(path),
            "Miscellaneous",
            true,
            new CustomScreenFilePath(path),
            CustomScreenType.Carolina
        );

    private static Guid GetScreenId(string path) =>
        new(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(path)));

    private static string GetScreenName(string path)
    {
        var indexFile = Directory.GetFiles(path, "*.htm*")
            .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == "index");

        if (indexFile == null)
            return Path.GetFileName(path) ?? "Unknown";

        var pageTitleMatch = TitleRegex().Match(File.ReadAllText(indexFile));

        if (!pageTitleMatch.Groups.TryGetValue("title", out var pageTitle) || string.IsNullOrWhiteSpace(pageTitle.Value))
            return Path.GetDirectoryName(path) ?? "Unknown";

        return pageTitle.Value;
    }
}