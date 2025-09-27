using jamster.engine.Domain;
using jamster.engine.Reducers;
using jamster.engine.Services;

namespace jamster.engine.Serialization;

public interface IScoreSheetSerializer
{
    ScoreSheetCollection Serialize(IGameStateStore stateStore);
}

[Singleton]
public class ScoreSheetSerializer : IScoreSheetSerializer
{
    public ScoreSheetCollection Serialize(IGameStateStore stateStore)
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