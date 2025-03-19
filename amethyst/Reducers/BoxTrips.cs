using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

//TODO: Handle skater added to bench while in box
public abstract class BoxTrips(TeamSide teamSide, ReducerGameContext context, ILogger logger) 
    : Reducer<BoxTripsState>(context)
    , IHandlesEvent<SkaterSatInBox>
    , IHandlesEvent<SkaterReleasedFromBox>
    , IHandlesEvent<SkaterSubstitutedInBox>
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , ITickReceiver
    , IDependsOnState<GameStageState>
{
    protected override BoxTripsState DefaultState => new([]);
    public override Option<string> GetStateKey() => Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(SkaterSatInBox @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();
        var gameStage = GetState<GameStageState>();

        var alreadyRunning = state.BoxTrips.Any(t => t.DurationInJams == null && t.SkaterNumber == @event.Body.SkaterNumber);

        if (alreadyRunning)
            return [];

        logger.LogDebug("Skater {skaterNumber} from {team} team sitting in box", @event.Body.SkaterNumber, teamSide);

        SetState(state with 
        { 
            BoxTrips = state.BoxTrips.Append(new(
                    gameStage.PeriodNumber,
                    gameStage.Stage == Stage.Jam ? gameStage.JamNumber : gameStage.JamNumber + 1,
                    gameStage.Stage == Stage.Jam ? gameStage.TotalJamNumber : gameStage.TotalJamNumber + 1,
                    @event.Body.SkaterNumber,
                    null,
                    [],
                    @event.Tick,
                    0,
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
                            SecondsPassed = ((Tick)(t.TicksPassedAtLastStart + @event.Tick - t.LastStartTick)).Seconds,
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
        
        SetState(new(
            state.BoxTrips.Select(t => 
                t.DurationInJams is null && (t.Substitutions.LastOrDefault()?.NewNumber ?? t.SkaterNumber) == @event.Body.OriginalSkaterNumber
                    ? t with { Substitutions = t.Substitutions.Append(new(@event.Body.NewSkaterNumber, totalJamNumber)).ToArray() }
                    : t
                ).ToArray()
        ));

        return [];
    });

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();

        SetState(new(
            state.BoxTrips.Select(t => t.DurationInJams is null
                ? t with
                {
                    LastStartTick = @event.Tick,
                    TicksPassedAtLastStart = t.TicksPassed,
                }
                : t)
            .ToArray()
        ));

        return [];
    }

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        var state = GetState();

        SetState(new(
            state.BoxTrips.Select(t => t.DurationInJams is null
                    ? t with
                    {
                        TicksPassed = t.TicksPassedAtLastStart + @event.Tick - t.LastStartTick,
                        SecondsPassed = ((Tick)(t.TicksPassedAtLastStart + @event.Tick - t.LastStartTick)).Seconds,
                    }
                    : t)
                .ToArray()
        ));

        return [];
    }

    public IEnumerable<Event> Tick(Tick tick)
    {
        var gameStage = GetState<GameStageState>();

        if (gameStage.Stage != Stage.Jam)
            return [];

        var state = GetState();

        SetState(new(
            state.BoxTrips.Select(t => t.DurationInJams is null
                    ? t with
                    {
                        TicksPassed = t.TicksPassedAtLastStart + tick - t.LastStartTick,
                        SecondsPassed = (t.TicksPassedAtLastStart + tick - t.LastStartTick).Seconds,
                    }
                    : t)
                .ToArray()
        ));

        return [];
    }
}

public sealed class HomeBoxTrips(ReducerGameContext context, ILogger<HomeBoxTrips> logger)
    : BoxTrips(TeamSide.Home, context, logger);

public sealed class AwayBoxTrips(ReducerGameContext context, ILogger<AwayBoxTrips> logger)
    : BoxTrips(TeamSide.Away, context, logger);

public sealed record BoxTripsState(BoxTrip[] BoxTrips)
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
    string SkaterNumber,
    int? DurationInJams,
    Substitution[] Substitutions,
    long LastStartTick,
    long TicksPassedAtLastStart,
    [property: IgnoreChange] long TicksPassed,
    int SecondsPassed)
{
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
        && other.TicksPassed.Equals(TicksPassed)
        && other.SecondsPassed.Equals(SecondsPassed);

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
        hashCode.Add(SecondsPassed);
        return hashCode.ToHashCode();
    }
}

public sealed record Substitution(string NewNumber, int TotalJamNumber);