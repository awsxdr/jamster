using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Events;

namespace amethyst.Services;

public interface IGameImporter
{
    Task<GameInfo> Import(StatsBook statsBook);
}

public class GameImporter(IGameDiscoveryService gameDiscoveryService, IEventBus eventBus) : IGameImporter
{
    private static readonly Dictionary<string, TeamColor> KnownColors = new (string[] Keys, (Color ShirtColor, Color ComplementaryColor) Colors)[]
    {
        (["red"], (Color.FromRgb(255, 0, 0), Color.White)),
        (["pink"], (Color.FromRgb(0xff, 0x88, 0x88), Color.Black)),
        (["orange"], (Color.FromRgb(0xff, 0x88, 0x00), Color.Black)),
        (["yellow"], (Color.FromRgb(0xff, 0xff, 0x00), Color.Black)),
        (["gold"], (Color.FromRgb(0x88, 0x88, 0x00), Color.Black)),
        (["brown"], (Color.FromRgb(0x88, 0x44, 0x00), Color.White)),
        (["lime"], (Color.FromRgb(0x88, 0xff, 0x00), Color.Black)),
        (["green"], (Color.FromRgb(0x00, 0xaa, 0x00), Color.White)),
        (["teal", "turquoise"], (Color.FromRgb(0x00, 0x88, 0x88), Color.Black)),
        (["blue"], (Color.FromRgb(0x00, 0x00, 0xff), Color.White)),
        (["purple"], (Color.FromRgb(0x88, 0x00, 0xff), Color.White)),
        (["black"], (Color.Black, Color.White)),
        (["grey", "gray"], (Color.FromRgb(0x66, 0x66, 0x66), Color.White)),
        (["white"], (Color.White, Color.Black)),
    }
    .SelectMany(x => x.Keys.Select(key => (Key: key, x.Colors.ShirtColor, x.Colors.ComplementaryColor)))
    .ToDictionary(x => x.Key, x => new TeamColor(x.ShirtColor, x.ComplementaryColor));

    public async Task<GameInfo> Import(StatsBook statsBook)
    {
        var gameName = $"{statsBook.Igrf.GameDetails.GameStart.Date:yyyy-MM-dd} - {GetTeamName(statsBook.Igrf.Teams.HomeTeam)} vs {GetTeamName(statsBook.Igrf.Teams.AwayTeam)}";
        if (!string.IsNullOrWhiteSpace(statsBook.Igrf.GameDetails.GameNumber))
            gameName += $" ({statsBook.Igrf.GameDetails.GameNumber})";

        var game = await gameDiscoveryService.GetGame(new(Guid.NewGuid(), gameName));

        await eventBus.AddEvent(game, new TeamSet(0, new TeamSetBody(TeamSide.Home, StatsBookTeamToGameTeam(statsBook.Igrf.Teams.HomeTeam))));
        await eventBus.AddEvent(game, new TeamSet(0, new TeamSetBody(TeamSide.Away, StatsBookTeamToGameTeam(statsBook.Igrf.Teams.AwayTeam))));

        return game;

        string GetTeamName(StatsBookTeam team) =>
            string.IsNullOrWhiteSpace(team.TeamName) ? team.LeagueName : team.TeamName;

        GameTeam StatsBookTeamToGameTeam(StatsBookTeam team) =>
            new(
                new()
                {
                    ["league"] = team.LeagueName,
                    ["team"] = team.TeamName,
                    ["color"] = team.ColorName,
                },
                ParseColor(team.ColorName),
                team.Skaters.Select(skater => new GameSkater(skater.Number, skater.Name, skater.IsSkating)).ToList()
            );

        TeamColor ParseColor(string colorName)
        {
            var normalizedColorName = colorName.Trim().ToLowerInvariant();

            return KnownColors.TryGetValue(normalizedColorName, out var knownColor)
                ? knownColor
                : new(Color.Black, Color.White);
        }
    }
}