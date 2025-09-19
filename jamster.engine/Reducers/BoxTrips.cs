using jamster.Domain;
using jamster.Events;
using jamster.Services;
// ReSharper disable WithExpressionModifiesAllMembers

namespace jamster.Reducers;

//TODO: Handle skater added to bench while in box
public abstract class BoxTrips(TeamSide teamSide, ReducerGameContext context, ILogger logger) 
    : Reducer<BoxTripsState>(context)
    , IHandlesEvent<SkaterSatInBox>
    , IHandlesEvent<SkaterReleasedFromBox>
    , IHandlesEvent<SkaterSubstitutedInBox>
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<StarPassMarked>
    , ITickReceiver
    , IDependsOnState<GameStageState>
    , IDependsOnState<JamLineupState>
{
    protected override BoxTripsState DefaultState => new([], false);
    public override Option<string> GetStateKey() => Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(SkaterSatInBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();
        var gameStage = GetState<GameStageState>();

        var alreadyRunning = state.BoxTrips.Any(t => t.DurationInJams == null && t.SkaterNumber == @event.Body.SkaterNumber);

        if (alreadyRunning)
            return [];

        var jamLineup = GetKeyedState<JamLineupState>(@event.Body.TeamSide.ToString());

        if (!jamLineup.Contains(@event.Body.SkaterNumber))
        {
            logger.LogWarning("Attempt to add skater {skaterNumber} on {team} team to box when skater not on track", @event.Body.SkaterNumber, @event.Body.TeamSide);
            return [];
        }

        var skaterPosition =
            jamLineup.JammerNumber == @event.Body.SkaterNumber ? SkaterPosition.Jammer
            : jamLineup.PivotNumber == @event.Body.SkaterNumber ? SkaterPosition.Pivot
            : SkaterPosition.Blocker;

        logger.LogDebug("Skater {skaterNumber} from {team} team sitting in box", @event.Body.SkaterNumber, teamSide);

        SetState(state with 
        { 
            BoxTrips = state.BoxTrips.Append(new(
                    gameStage.PeriodNumber,
                    gameStage.Stage == Stage.Jam ? gameStage.JamNumber : gameStage.JamNumber + 1,
                    gameStage.Stage == Stage.Jam ? gameStage.TotalJamNumber : gameStage.TotalJamNumber + 1,
                    state.HasStarPassInJam,
                    gameStage.Stage != Stage.Jam,
                    @event.Body.SkaterNumber,
                    skaterPosition,
                    null,
                    false,
                    [],
                    @event.Tick,
                    0,
                    0
                )).ToArray()
        });

        return [];
    });

    public IEnumerable<Event> Handle(SkaterReleasedFromBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        logger.LogDebug("Skater {skaterNumber} from {team} team released from box", @event.Body.SkaterNumber, teamSide);

        var state = GetState();
        var gameStage = GetState<GameStageState>();

        SetState(state with 
            {
                BoxTrips = state.BoxTrips
                    .Select(t => t.DurationInJams is null && t.SkaterNumber == @event.Body.SkaterNumber
                        ? t with
                        {
                            DurationInJams = gameStage.TotalJamNumber - t.TotalJamStart,
                            TicksPassed = t.TicksPassedAtLastStart + @event.Tick - t.LastStartTick,
                        }
                        : t)
                    .ToArray(),
            });

        return [];
    });

    public IEnumerable<Event> Handle(SkaterSubstitutedInBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();
        var gameStage = GetState<GameStageState>();

        var totalJamNumber = gameStage.Stage == Stage.Jam ? gameStage.TotalJamNumber : gameStage.TotalJamNumber + 1;
        
        SetState(state with
        {
            BoxTrips = state.BoxTrips.Select(t => 
                t.DurationInJams is null && (t.Substitutions.LastOrDefault()?.NewNumber ?? t.SkaterNumber) == @event.Body.OriginalSkaterNumber
                    ? t with { Substitutions = t.Substitutions.Append(new(@event.Body.NewSkaterNumber, totalJamNumber)).ToArray() }
                    : t
                ).ToArray(),
        });

        return [];
    });

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();

        SetState(state with
        {
            BoxTrips = state.BoxTrips.Select(t => t.DurationInJams is null
                ? t with
                {
                    LastStartTick = @event.Tick,
                    TicksPassedAtLastStart = t.TicksPassed,
                }
                : t)
            .ToArray(),
            HasStarPassInJam = false,
        });

        return [];
    }

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        var state = GetState();

        SetState(state with
        {
            BoxTrips = state.BoxTrips.Select(t => t.DurationInJams is null
                    ? t with
                    {
                        TicksPassed = t.TicksPassedAtLastStart + @event.Tick - t.LastStartTick,
                    }
                    : t)
                .ToArray()
        });

        return [];
    }

    public IEnumerable<Event> Handle(StarPassMarked @event) => @event.HandleIfTeam(teamSide, () =>
    {
        SetState(GetState() with { HasStarPassInJam = @event.Body.StarPass });

        return [];
    });

    public IEnumerable<Event> Tick(Tick tick)
    {
        var gameStage = GetState<GameStageState>();

        if (gameStage.Stage != Stage.Jam)
            return [];

        var state = GetState();

        SetState(state with
        {
            BoxTrips = state.BoxTrips.Select(t => t.DurationInJams is null
                    ? t with
                    {
                        TicksPassed = t.TicksPassedAtLastStart + tick - t.LastStartTick,
                    }
                    : t)
                .ToArray()
        });

        return [];
    }
}

public sealed class HomeBoxTrips(ReducerGameContext context, ILogger<HomeBoxTrips> logger)
    : BoxTrips(TeamSide.Home, context, logger);

public sealed class AwayBoxTrips(ReducerGameContext context, ILogger<AwayBoxTrips> logger)
    : BoxTrips(TeamSide.Away, context, logger);

public sealed record BoxTripsState(BoxTrip[] BoxTrips, bool HasStarPassInJam)
{
    public bool Equals(BoxTripsState? other) =>
        other is not null 
        && other.BoxTrips.SequenceEqual(BoxTrips);

    public override int GetHashCode() => BoxTrips.GetHashCode();
}

public sealed record BoxTrip(
    int Period,
    int Jam,
    int TotalJamStart,
    bool StartAfterStarPass,
    bool StartBetweenJams,
    string SkaterNumber,
    SkaterPosition SkaterPosition,
    int? DurationInJams,
    bool EndAfterStarPass,
    Substitution[] Substitutions,
    Tick LastStartTick,
    Tick TicksPassedAtLastStart,
    [property: IgnoreChange] Tick TicksPassed)
{
    public int SecondsPassed => TicksPassed.Seconds;

    public bool Equals(BoxTrip? other) =>
        other is not null
        && other.Period.Equals(Period)
        && other.Jam.Equals(Jam)
        && other.TotalJamStart.Equals(TotalJamStart)
        && other.SkaterNumber.Equals(SkaterNumber)
        && (other.DurationInJams?.Equals(DurationInJams) ?? DurationInJams is null)
        && other.Substitutions.SequenceEqual(Substitutions)
        && other.LastStartTick.Equals(LastStartTick)
        && other.TicksPassedAtLastStart.Equals(TicksPassedAtLastStart)
        && other.TicksPassed.Equals(TicksPassed);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Period);
        hashCode.Add(Jam);
        hashCode.Add(TotalJamStart);
        hashCode.Add(SkaterNumber);
        hashCode.Add(DurationInJams);
        hashCode.Add(Substitutions);
        hashCode.Add(LastStartTick);
        hashCode.Add(TicksPassedAtLastStart);
        hashCode.Add(TicksPassed);
        return hashCode.ToHashCode();
    }
}

public sealed record Substitution(string NewNumber, int TotalJamNumber);