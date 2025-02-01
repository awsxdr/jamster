using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Reducers;

namespace amethyst.Services;

public interface IGameExporter
{
    StatsBook Export(GameInfo game);
}

[Singleton]
public class GameExporter(IGameContextFactory contextFactory) : IGameExporter
{
    public StatsBook Export(GameInfo game)
    {
        var context = contextFactory.GetGame(game);

        return new StatsBook(
            GetIgrf(context.StateStore),
            GetScoreSheets(context.StateStore)
        );
    }

    private Igrf GetIgrf(IGameStateStore stateStore)
    {
        var homeTeam = stateStore.GetKeyedState<TeamDetailsState>(nameof(TeamSide.Home));
        var awayTeam = stateStore.GetKeyedState<TeamDetailsState>(nameof(TeamSide.Away));

        return new(
            GetGameLocation(),
            GetGameDetails(),
            new(GetTeam(homeTeam), GetTeam(awayTeam)),
            GetGameSummary(stateStore)
        );
    }

    private GameLocation GetGameLocation() =>
        new("", "", "");

    private GameDetails GetGameDetails() =>
        new("", "", "", DateTime.UtcNow);

    private StatsBookTeam GetTeam(TeamDetailsState team) =>
        new(
            team.Team.Names.GetValueOrDefault("league", ""),
            team.Team.Names.GetValueOrDefault("team", ""),
            team.Team.Names.GetValueOrDefault("color", ""),
            team.Team.Roster.Select(skater => new StatsBookSkater(skater.Number, skater.Name, skater.IsSkating)).ToArray()
        );

    private GameSummary GetGameSummary(IGameStateStore stateStore)
    {
        var homeScoreSheet = stateStore.GetKeyedState<ScoreSheetState>(nameof(TeamSide.Home));
        var awayScoreSheet = stateStore.GetKeyedState<ScoreSheetState>(nameof(TeamSide.Away));

        var homePeriod1Total = homeScoreSheet.Jams.LastOrDefault(j => j.Period == 1)?.GameTotal ?? 0;
        var awayPeriod1Total = awayScoreSheet.Jams.LastOrDefault(j => j.Period == 1)?.GameTotal ?? 0;
        var homePeriod2Total = homeScoreSheet.Jams.LastOrDefault(j => j.Period == 2)?.GameTotal ?? homePeriod1Total;
        var awayPeriod2Total = awayScoreSheet.Jams.LastOrDefault(j => j.Period == 2)?.GameTotal ?? awayPeriod1Total;

        return new(
            new(0, homePeriod1Total, 0, awayPeriod1Total),
            new(0, homePeriod2Total - homePeriod1Total, 0, awayPeriod2Total - awayPeriod1Total)
        );
    }

    private ScoreSheetCollection GetScoreSheets(IGameStateStore stateStore)
    {
        var homeScoreSheet = stateStore.GetKeyedState<ScoreSheetState>(nameof(TeamSide.Home));
        var awayScoreSheet = stateStore.GetKeyedState<ScoreSheetState>(nameof(TeamSide.Away));

        var homeJams = GetJamsWithOpponentJams(homeScoreSheet.Jams, awayScoreSheet.Jams);
        var awayJams = GetJamsWithOpponentJams(awayScoreSheet.Jams, homeScoreSheet.Jams);

        return new(
            new("", "", homeJams.Where(j => j.PeriodNumber == 1).SelectMany(GetScoreSheetLines).ToArray()),
            new("", "", awayJams.Where(j => j.PeriodNumber == 1).SelectMany(GetScoreSheetLines).ToArray()),
            new("", "", homeJams.Where(j => j.PeriodNumber == 2).SelectMany(GetScoreSheetLines).ToArray()),
            new("", "", awayJams.Where(j => j.PeriodNumber == 2).SelectMany(GetScoreSheetLines).ToArray())
        );

        JamWithOpponentJam[] GetJamsWithOpponentJams(ScoreSheetJam[] jams, ScoreSheetJam[] opponentJams) =>
            jams
                .Zip(opponentJams)
                .Select(j => new JamWithOpponentJam(
                    PeriodNumber: j.First.Period == j.Second.Period ? j.First.Period : throw new TeamSheetsDoNotMatchException(),
                    Jam: j.First,
                    OpponentJam: j.Second))
                .ToArray();
    }

    private ScoreSheetLine[] GetScoreSheetLines(JamWithOpponentJam jam)
    {
        var starPassInJam = jam.Jam.StarPassTrip != null || jam.OpponentJam.StarPassTrip != null;

        return starPassInJam
            ? [GetPreStarPassJamLine(jam.Jam), GetPostStarPassJamLine(jam.Jam)]
            : [GetNonStarPassJamLine(jam.Jam)];
    }

    private static ScoreSheetLine GetNonStarPassJamLine(ScoreSheetJam jam) =>
        new(
            jam.Jam,
            jam.JammerNumber,
            jam.Lost,
            jam.Lead,
            jam.Called,
            jam.Injury,
            jam.NoInitial,
            Enumerable.Range(0, 9)
                .Select(i => i < jam.Trips.Length ? jam.Trips[i].Score : null)
                .Select(s => new ScoreSheetTrip(s))
                .ToArray()
        );

    private static ScoreSheetLine GetPreStarPassJamLine(ScoreSheetJam jam) =>
        new(
            jam.Jam,
            jam.JammerNumber,
            jam.Lost,
            jam.Lead,
            jam.Lead,
            jam.StarPassTrip == null && jam.Injury,
            jam.StarPassTrip == 0 || jam.StarPassTrip == null && jam.NoInitial,
            Enumerable.Range(0, 9)
                .Select(i => i < (jam.StarPassTrip ?? 10) && i < jam.Trips.Length ? jam.Trips[i].Score : null)
                .Select(s => new ScoreSheetTrip(s))
                .ToArray()
        );

    private static ScoreSheetLine GetPostStarPassJamLine(ScoreSheetJam jam) =>
        new(
            jam.StarPassTrip == null ? "SP*" : "SP",
            jam.StarPassTrip == null ? "" : jam.PivotNumber,
            false,
            false,
            false,
            jam is { StarPassTrip: not null, Injury: true },
            jam is { StarPassTrip: not null, NoInitial: true },
            Enumerable.Range(0, 9)
                .Select(i => i >= jam.StarPassTrip && i < jam.Trips.Length ? jam.Trips[i].Score : null)
                .Select(s => new ScoreSheetTrip(s))
                .ToArray()
        );

    private record JamWithOpponentJam(int PeriodNumber, ScoreSheetJam Jam, ScoreSheetJam OpponentJam);
}

public sealed class TeamSheetsDoNotMatchException : Exception;