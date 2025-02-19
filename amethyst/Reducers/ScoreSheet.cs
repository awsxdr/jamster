using amethyst.Domain;
using amethyst.Events;
using amethyst.Extensions;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class ScoreSheet(TeamSide teamSide, ReducerGameContext context)
    : Reducer<ScoreSheetState>(context)
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<SkaterAddedToJam>
    , IHandlesEvent<SkaterRemovedFromJam>
    , IHandlesEvent<InitialTripCompleted>
    , IHandlesEvent<TripCompleted>
    , IHandlesEvent<ScoreModifiedRelative>
    , IHandlesEvent<LeadMarked>
    , IHandlesEvent<LostMarked>
    , IHandlesEvent<CallMarked>
    , IHandlesEvent<StarPassMarked>
    , IHandlesEvent<ScoreSheetJammerNumberSet>
    , IHandlesEvent<ScoreSheetPivotNumberSet>
    , IHandlesEvent<ScoreSheetTripScoreSet>
    , IHandlesEvent<ScoreSheetLeadSet>
    , IHandlesEvent<ScoreSheetLostSet>
    , IHandlesEvent<ScoreSheetCalledSet>
    , IHandlesEvent<ScoreSheetInjurySet>
    , IHandlesEvent<ScoreSheetNoInitialSet>
    , IHandlesEvent<ScoreSheetStarPassTripSet>
    , IDependsOnState<JamLineupState>
    , IDependsOnState<GameStageState>
    , IDependsOnState<TeamJamStatsState>
{
    protected override ScoreSheetState DefaultState => new([]);

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();
        var lineup = GetKeyedState<JamLineupState>(teamSide.ToString());
        var gameStage = GetState<GameStageState>();

        SetState(new(state.Jams.Append(new ScoreSheetJam(
            gameStage.PeriodNumber,
            gameStage.JamNumber,
            lineup.JammerNumber ?? "?",
            lineup.PivotNumber ?? "?",
            false,
            false,
            false,
            false,
            true,
            [],
            null,
            0,
            state.Jams.Any() ? state.Jams[^1].GameTotal : 0
        )).ToArray()));

        return [];
    }

    public IEnumerable<Event> Handle(SkaterAddedToJam @event) => @event.HandleIfTeam(teamSide, () =>
    {
        switch (@event.Body.Position)
        {
            case SkaterPosition.Jammer:
                ModifyJam(@event.Body.Period, @event.Body.Jam, jam => jam with { JammerNumber = @event.Body.SkaterNumber });
                break;

            case SkaterPosition.Pivot:
                ModifyJam(@event.Body.Period, @event.Body.Jam, jam => jam with { PivotNumber = @event.Body.SkaterNumber });
                break;
        }

        return [];
    });

    public IEnumerable<Event> Handle(SkaterRemovedFromJam @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyJam(@event.Body.Period, @event.Body.Jam, jam => jam with
        {
            JammerNumber = jam.JammerNumber == @event.Body.SkaterNumber ? string.Empty : jam.JammerNumber,
            PivotNumber = jam.PivotNumber == @event.Body.SkaterNumber ? string.Empty : jam.PivotNumber,
        });

        return [];
    });

    public IEnumerable<Event> Handle(InitialTripCompleted @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var jamStatsState = GetKeyedState<TeamJamStatsState>(teamSide.ToString());

        ModifyLatestJam(jam => jam with
        {
            NoInitial = !jamStatsState.HasCompletedInitial,
            Trips = jamStatsState.HasCompletedInitial ? [new JamLineTrip(null)] : [],
        });

        return [];
    });

    public IEnumerable<Event> Handle(TripCompleted @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyLatestJam(jam => jam with { Trips = jam.Trips.Append(new(null)).ToArray() });

        return [];
    });

    public IEnumerable<Event> Handle(ScoreModifiedRelative @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyLatestJam(jam => jam with
        {
            JamTotal = jam.JamTotal + @event.Body.Value,
            GameTotal = jam.GameTotal + @event.Body.Value,
            Trips = jam.Trips.Any()
                ? jam.Trips.Take(jam.Trips.Length - 1).Append(jam.Trips[^1] with
                    {
                        Score = (jam.Trips[^1].Score ?? 0) + @event.Body.Value,
                    }).ToArray()
                : [new JamLineTrip(@event.Body.Value)]
        });

        return [];
    });

    public IEnumerable<Event> Handle(LeadMarked @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyLatestJam(jam => jam with { Lead = @event.Body.Lead });

        return [];
    });

    public IEnumerable<Event> Handle(LostMarked @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyLatestJam(jam => jam with { Lost = @event.Body.Lost });

        return [];
    });

    public IEnumerable<Event> Handle(CallMarked @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyLatestJam(jam => jam with { Called = @event.Body.Call });

        return [];
    });

    public IEnumerable<Event> Handle(StarPassMarked @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyLatestJam(jam => jam with
        {
            StarPassTrip = @event.Body.StarPass ? jam.Trips.Length : null,
        });

        return [];
    });

    public IEnumerable<Event> Handle(ScoreSheetJammerNumberSet @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyJam(@event.Body.TotalJamNumber, line => line with { JammerNumber = @event.Body.Value });

        return [];
    });

    public IEnumerable<Event> Handle(ScoreSheetPivotNumberSet @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyJam(@event.Body.TotalJamNumber, line => line with { PivotNumber = @event.Body.Value });

        return [];
    });

    public IEnumerable<Event> Handle(ScoreSheetTripScoreSet @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyJam(@event.Body.TotalJamNumber, jam =>
        {
            if (@event.Body.TripNumber > jam.Trips.Length || @event.Body.TripNumber < 0)
                return jam;

            var trips = jam.Trips.ToArray();
            if (@event.Body.TripNumber == trips.Length)
                trips = trips.Append(new(@event.Body.Value)).ToArray();
            else
                trips[@event.Body.TripNumber] = new(@event.Body.Value);

            trips = trips.Where(t => t.Score != null).ToArray();

            return jam with
            {
                Trips = trips.Where(t => t.Score != null).ToArray(),
            };
        });

        var state = GetState();
        SetState(new(state.Jams
            .Aggregate(Array.Empty<ScoreSheetJam>(), (list, jam) => list.Append(jam with
            {
                JamTotal = jam.Trips.Sum(t => t.Score ?? 0),
            }).ToArray())
            .Aggregate(Array.Empty<ScoreSheetJam>(), (list, jam) => list.Append(jam with
            {
                GameTotal = list.Any() ? list[^1].GameTotal + jam.JamTotal : jam.JamTotal
            }).ToArray())
        ));

        return [];
    });

    public IEnumerable<Event> Handle(ScoreSheetLeadSet @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyJam(@event.Body.TotalJamNumber, line => line with { Lead = @event.Body.Value });

        return [];
    });

    public IEnumerable<Event> Handle(ScoreSheetLostSet @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyJam(@event.Body.TotalJamNumber, line => line with { Lost = @event.Body.Value });

        return [];
    });

    public IEnumerable<Event> Handle(ScoreSheetCalledSet @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyJam(@event.Body.TotalJamNumber, line => line with { Called = @event.Body.Value });

        return [];
    });

    public IEnumerable<Event> Handle(ScoreSheetInjurySet @event)
    {
        ModifyJam(@event.Body.TotalJamNumber, line => line with { Injury = @event.Body.Value });

        return [];
    }

    public IEnumerable<Event> Handle(ScoreSheetNoInitialSet @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyJam(@event.Body.TotalJamNumber, line => line with { NoInitial = @event.Body.Value });

        return [];
    });

    public IEnumerable<Event> Handle(ScoreSheetStarPassTripSet @event) => @event.HandleIfTeam(teamSide, () =>
    {
        ModifyJam(@event.Body.TotalJamNumber, line => line with
        {
            StarPassTrip = @event.Body.StarPassTrip == null ? null : Math.Min(line.Trips.Length, (int)@event.Body.StarPassTrip!),
        });

        return [];
    });

    private void ModifyLatestJam(Func<ScoreSheetJam, ScoreSheetJam> mapper)
    {
        var state = GetState();

        if (!state.Jams.Any())
            return;

        SetState(new(
            state.Jams.Take(state.Jams.Length - 1)
                .Append(mapper(state.Jams[^1]))
                .ToArray()));
    }

    private void ModifyJam(int period, int jam, Func<ScoreSheetJam, ScoreSheetJam> mapper)
    {
        var state = GetState();

        var totalJamNumber = state.Jams.Select((jam, totalJamNumber) => (Jam: jam, TotalJamNumber: totalJamNumber)).Where(j => j.Jam.Jam == jam && j.Jam.Period == period).Select(j => j.TotalJamNumber).ToArray();

        if (!totalJamNumber.Any())
            return;

        ModifyJam(totalJamNumber[0], mapper);
    }

    private void ModifyJam(int totalJamNumber, Func<ScoreSheetJam, ScoreSheetJam> mapper)
    {
        var state = GetState();

        if (totalJamNumber >= state.Jams.Length || totalJamNumber < 0)
            return;

        SetState(new(
            state.Jams.Take(totalJamNumber)
                .Append(mapper(state.Jams[totalJamNumber]))
                .Concat(state.Jams.Skip(totalJamNumber + 1))
                .ToArray()));
    }
}

public sealed record ScoreSheetState(ScoreSheetJam[] Jams)
{
    public bool Equals(ScoreSheetState? other) =>
        other is not null
        && other.Jams.SequenceEqual(Jams);

    public override int GetHashCode() => Jams.GetHashCode();
}

public sealed record ScoreSheetJam(
    int Period,
    int Jam,
    string JammerNumber,
    string PivotNumber,
    bool Lost,
    bool Lead,
    bool Called,
    bool Injury,
    bool NoInitial,
    JamLineTrip[] Trips,
    int? StarPassTrip,
    int JamTotal,
    int GameTotal
)
{
    public bool Equals(ScoreSheetJam? other) =>
        other is not null
        && other.Period.Equals(Period)
        && other.Jam.Equals(Jam)
        && other.JammerNumber.Equals(JammerNumber)
        && other.PivotNumber.Equals(PivotNumber)
        && other.Lost.Equals(Lost)
        && other.Lead.Equals(Lead)
        && other.Called.Equals(Called)
        && other.Injury.Equals(Injury)
        && other.NoInitial.Equals(NoInitial)
        && other.Trips.SequenceEqual(Trips)
        && other.StarPassTrip.Equals(StarPassTrip)
        && other.JamTotal.Equals(JamTotal)
        && other.GameTotal.Equals(GameTotal);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Period);
        hashCode.Add(Jam);
        hashCode.Add(JammerNumber);
        hashCode.Add(PivotNumber);
        hashCode.Add(Lost);
        hashCode.Add(Lead);
        hashCode.Add(Called);
        hashCode.Add(Injury);
        hashCode.Add(NoInitial);
        hashCode.Add(Trips);
        hashCode.Add(StarPassTrip);
        hashCode.Add(JamTotal);
        hashCode.Add(GameTotal);
        return hashCode.ToHashCode();
    }
}
public record JamLineTrip(int? Score);

public sealed class HomeScoreSheet(ReducerGameContext gameContext)
    : ScoreSheet(TeamSide.Home, gameContext);
public sealed class AwayScoreSheet(ReducerGameContext gameContext)
    : ScoreSheet(TeamSide.Away, gameContext);