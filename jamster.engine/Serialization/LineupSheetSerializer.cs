using jamster.engine.Domain;
using jamster.engine.Extensions;
using jamster.engine.Reducers;
using jamster.engine.Services;

namespace jamster.engine.Serialization;

public interface ILineupSheetSerializer
{
    LineupSheetCollection Serialize(IGameStateStore stateStore);
}

[Singleton]
public class LineupSheetSerializer : ILineupSheetSerializer
{
    public LineupSheetCollection Serialize(IGameStateStore stateStore)
    {
        var homeLineupSheet = stateStore.GetKeyedState<LineupSheetState>(nameof(TeamSide.Home));
        var awayLineupSheet = stateStore.GetKeyedState<LineupSheetState>(nameof(TeamSide.Away));
        var homeBoxTrips = stateStore.GetKeyedState<BoxTripsState>(nameof(TeamSide.Home));
        var awayBoxTrips = stateStore.GetKeyedState<BoxTripsState>(nameof(TeamSide.Away));

        var homeJams = homeLineupSheet.Jams.Zip(awayLineupSheet.Jams, (l, r) => new ExportableLineupJam(l, l.HasStarPass || r.HasStarPass, 0)).Select((x, i) => x with { TotalJam = i + 1 }).ToArray();
        var awayJams = awayLineupSheet.Jams.Zip(homeLineupSheet.Jams, (l, r) => new ExportableLineupJam(l, l.HasStarPass || r.HasStarPass, 0)).Select((x, i) => x with { TotalJam = i + 1 }).ToArray();

        return new(
            new("", homeJams.Where(j => j.Period == 1).SelectMany(GetLineupSheetLines(homeBoxTrips.BoxTrips)).ToArray()),
            new("", awayJams.Where(j => j.Period == 1).SelectMany(GetLineupSheetLines(awayBoxTrips.BoxTrips)).ToArray()),
            new("", homeJams.Where(j => j.Period == 2).SelectMany(GetLineupSheetLines(homeBoxTrips.BoxTrips)).ToArray()),
            new("", awayJams.Where(j => j.Period == 2).SelectMany(GetLineupSheetLines(awayBoxTrips.BoxTrips)).ToArray())
        );
    }

    private static Func<ExportableLineupJam, LineupSheetLine[]> GetLineupSheetLines(BoxTrip[] boxTrips) => jam =>
        jam.EitherTeamHasStarPass
            ? [GetLineupSheetLine(jam, boxTrips, false), GetLineupSheetLine(jam, boxTrips, true)]
            : [GetLineupSheetLine(jam, boxTrips, false)];

    private static LineupSheetLine GetLineupSheetLine(ExportableLineupJam jam, BoxTrip[] boxTrips, bool starPassLine) =>
        new(
            starPassLine ? jam.HasStarPass ? "SP" : "SP*" : jam.Jam,
            jam.HasStarPass && starPassLine || jam.BlockerNumbers.Length == 4,
            (jam switch
            {
                { EitherTeamHasStarPass: true, HasStarPass: false } when starPassLine => ["", "", "", "", ""],
                { HasStarPass: true } when starPassLine => new[] { jam.PivotNumber, jam.JammerNumber }.Concat(jam.BlockerNumbers.Take(3).OrderBy(x => x)),
                _ => new[] { jam.JammerNumber, jam.PivotNumber is null && jam.BlockerNumbers.Length == 4 ? jam.BlockerNumbers[3] : jam.PivotNumber }.Concat(jam.BlockerNumbers.Take(3).OrderBy(x => x))
            })
            .Pad(5, "?")
            .Select(s => GetLineupSkater(s, boxTrips.Where(b => b.SkaterNumber == s).ToArray(), jam, starPassLine))
            .ToArray());

    private static LineupSkater GetLineupSkater(string? skaterNumber, BoxTrip[] boxTrips, ExportableLineupJam jam, bool afterStarPass)
    {
        var relevantBoxTrips = boxTrips.Where(BoxTripIsRelevant).ToArray();

        return new(
            skaterNumber ?? "?",
            relevantBoxTrips.Take(3).Select(SymbolForBoxTrip).Pad(3, "").ToArray()
        );

        bool BoxTripIsRelevant(BoxTrip trip)
        {
            var tripStartsThisJam = trip.TotalJamStart == jam.TotalJam;
            var tripStartsBeforeThisJam = trip.TotalJamStart < jam.TotalJam;
            var tripEndsThisJam = trip.TotalJamStart + (trip.DurationInJams ?? 1000) == jam.TotalJam;
            var tripEndsAfterThisJam = trip.TotalJamStart + (trip.DurationInJams ?? 1000) > jam.TotalJam;
            var tripStartsBeforeStarPassAndRunsForMultipleJams = !trip.StartAfterStarPass && afterStarPass && (trip.DurationInJams ?? 1000) > 0;
            var tripEndsThisJamAndStarPassMatches = tripEndsThisJam && trip.EndAfterStarPass == afterStarPass;
            var tripStartsBeforeJamAndEndsThisJam = tripStartsBeforeThisJam && tripEndsThisJam;

            return
                tripStartsBeforeThisJam && tripEndsAfterThisJam
                || tripStartsThisJam && (
                    !jam.HasStarPass
                    || tripStartsBeforeStarPassAndRunsForMultipleJams && afterStarPass
                    || trip.StartAfterStarPass == afterStarPass
                )
                || tripStartsBeforeJamAndEndsThisJam && (trip.EndAfterStarPass || !trip.EndAfterStarPass && !afterStarPass)
                || tripEndsThisJamAndStarPassMatches;
        }

        string SymbolForBoxTrip(BoxTrip boxTrip)
        {
            var startInBox =
                boxTrip.TotalJamStart < jam.TotalJam
                || boxTrip.StartBetweenJams && boxTrip.TotalJamStart == jam.TotalJam
                || boxTrip.TotalJamStart == jam.TotalJam && afterStarPass && !boxTrip.StartAfterStarPass;

            var endThisJam =
                boxTrip.DurationInJams != null
                && boxTrip.TotalJamStart + boxTrip.DurationInJams == jam.TotalJam
                && boxTrip.EndAfterStarPass == afterStarPass;

            return (startInBox, endThisJam) switch
            {
                (true, true) => "$",
                (true, false) => "S",
                (false, true) => "+",
                (false, false) => "-",
            };
        }
    }

    private record ExportableLineupJam(
        int Period,
        int Jam,
        bool HasStarPass,
        string? JammerNumber,
        string? PivotNumber,
        string?[] BlockerNumbers,
        bool EitherTeamHasStarPass,
        int TotalJam
    ) : LineupSheetJam(Period, Jam, HasStarPass, JammerNumber, PivotNumber, BlockerNumbers)
    {
        public ExportableLineupJam(LineupSheetJam jam, bool eitherTeamHasStarPass, int totalJam)
            : this(jam.Period, jam.Jam, jam.HasStarPass, jam.JammerNumber, jam.PivotNumber, jam.BlockerNumbers, eitherTeamHasStarPass, totalJam)
        {
        }
    }
}