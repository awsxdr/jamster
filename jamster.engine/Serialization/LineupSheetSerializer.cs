using jamster.engine.Domain;
using jamster.engine.Events;
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
        var homeTeam = stateStore.GetKeyedState<TeamDetailsState>(nameof(TeamSide.Home));
        var awayTeam = stateStore.GetKeyedState<TeamDetailsState>(nameof(TeamSide.Away));

        var homeJams = homeLineupSheet.Jams.Zip(awayLineupSheet.Jams, (l, r) => new ExportableLineupJam(l, l.HasStarPass || r.HasStarPass, 0)).Select((x, i) => x with { TotalJam = i + 1 }).ToArray();
        var awayJams = awayLineupSheet.Jams.Zip(homeLineupSheet.Jams, (l, r) => new ExportableLineupJam(l, l.HasStarPass || r.HasStarPass, 0)).Select((x, i) => x with { TotalJam = i + 1 }).ToArray();

        return new(
            new("", homeJams.Where(j => j.Period == 1).SelectMany(GetLineupSheetLines(homeBoxTrips.BoxTrips, homeTeam.Team)).ToArray()),
            new("", awayJams.Where(j => j.Period == 1).SelectMany(GetLineupSheetLines(awayBoxTrips.BoxTrips, awayTeam.Team)).ToArray()),
            new("", homeJams.Where(j => j.Period == 2).SelectMany(GetLineupSheetLines(homeBoxTrips.BoxTrips, homeTeam.Team)).ToArray()),
            new("", awayJams.Where(j => j.Period == 2).SelectMany(GetLineupSheetLines(awayBoxTrips.BoxTrips, awayTeam.Team)).ToArray())
        );
    }

    private static Func<ExportableLineupJam, LineupSheetLine[]> GetLineupSheetLines(BoxTrip[] boxTrips, GameTeam team) => jam =>
        jam.EitherTeamHasStarPass
            ? [GetLineupSheetLine(jam, boxTrips, team, false), GetLineupSheetLine(jam, boxTrips, team, true)]
            : [GetLineupSheetLine(jam, boxTrips, team, false)];

    private static LineupSheetLine GetLineupSheetLine(ExportableLineupJam jam, BoxTrip[] boxTrips, GameTeam team, bool starPassLine)
    {
        Union<int, string> jamLabel = starPassLine ? jam.HasStarPass ? "SP" : "SP*" : jam.Jam;
        var extraBlocker = jam.HasStarPass && starPassLine || jam.BlockerIds.Length == 4;

        if (jam is { EitherTeamHasStarPass: true, HasStarPass: false } && starPassLine)
            return new(jamLabel, extraBlocker, Enumerable.Repeat(new LineupSkater("", ["", "", ""]), 5).ToArray());

        var skaterIds = (Guid?[]) (
            jam.HasStarPass && starPassLine ? [jam.PivotId, jam.JammerId, ..jam.BlockerIds.Take(3)]
            : jam.PivotId == null && jam.BlockerIds.Length == 4 ? [jam.JammerId, ..jam.BlockerIds]
            : [jam.JammerId, jam.PivotId, ..jam.BlockerIds]);

        return new(
            jamLabel,
            extraBlocker,
            skaterIds
                .Select(s => GetLineupSkater(GetSkaterNumber(team, s), boxTrips.Where(b => b.SkaterId == s).ToArray(), jam, starPassLine))
                .Map(s => s.Take(2).Concat(s.Skip(2).OrderBy(a => a.Number)))
                .Pad(5, new LineupSkater("?", ["", "", ""]))
                .ToArray());
    }

    private static string GetSkaterNumber(GameTeam team, Guid? skaterId) =>
        skaterId is null
            ? "?"
            : team.Roster.FirstOrDefault(s => s.Id == skaterId)?.Number ?? "?";

    private static LineupSkater GetLineupSkater(string skaterNumber, BoxTrip[] boxTrips, ExportableLineupJam jam, bool afterStarPass)
    {
        var relevantBoxTrips = boxTrips.Where(BoxTripIsRelevant).ToArray();

        return new(
            skaterNumber,
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
        Guid? JammerId,
        Guid? PivotId,
        Guid?[] BlockerIds,
        bool EitherTeamHasStarPass,
        int TotalJam
    ) : LineupSheetJam(Period, Jam, HasStarPass, JammerId, PivotId, BlockerIds)
    {
        public ExportableLineupJam(LineupSheetJam jam, bool eitherTeamHasStarPass, int totalJam)
            : this(jam.Period, jam.Jam, jam.HasStarPass, jam.JammerId, jam.PivotId, jam.BlockerIds, eitherTeamHasStarPass, totalJam)
        {
        }
    }
}