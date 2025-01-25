using amethyst.Domain;
using amethyst.Events;
using amethyst.Extensions;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class TripScore(TeamSide teamSide, ReducerGameContext context, ILogger logger)
    : Reducer<TripScoreState>(context)
    , IHandlesEvent<ScoreModifiedRelative>
    , IHandlesEvent<ScoreSet>
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<LastTripDeleted>
    , IHandlesEvent<InitialTripCompleted>
    , IHandlesEvent<TripCompleted>
    , IDependsOnState<JamClockState>
    , ITickReceiver
{
    protected override TripScoreState DefaultState => new(null, 0, 0);

    public static readonly Tick TripScoreResetTimeInTicks = 3000;

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(ScoreModifiedRelative @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();
        var jamClock = GetState<JamClockState>();

        var keepCurrentValue = !jamClock.IsRunning || @event.Tick - state.LastScoreTick < TripScoreResetTimeInTicks;

        SetState(state with {
            Score = Math.Min(4, Math.Max(0,
                keepCurrentValue
                ? (state.Score ?? 0) + @event.Body.Value
                : @event.Body.Value)),
            LastScoreTick = @event.Tick,
        });

        return [];
    });

    public IEnumerable<Event> Handle(ScoreSet @event) => @event.HandleIfTeam(teamSide, () =>
    {
        logger.LogDebug("Resetting trip score due to absolute score being set");

        SetState(new(null, 0, @event.Tick));

        return [];
    });

    public IEnumerable<Event> Handle(JamStarted @event) => @event.HandleIfTeam(teamSide, () =>
    {
        logger.LogDebug("Resetting jam trip count due to jam start at tick {tick}", @event.Tick);

        SetState(GetState() with { Score = null, JamTripCount = 0 });

        return [];
    });

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        logger.LogDebug("Resetting trip score due to jam end");

        return GetState() switch
        {
            { JamTripCount: 0 } => [],
            { Score: null } => [new ScoreModifiedRelative(@event.Tick, new(teamSide, 0))],
            _ => 
            [
                new ScoreModifiedRelative(@event.Tick, new(teamSide, 0)),
                new TripCompleted(@event.Tick, new(teamSide))
            ]
        };

    }

    public IEnumerable<Event> Handle(LastTripDeleted @event) => @event.HandleIfTeam(teamSide, () =>
    {
        logger.LogDebug("Resetting trip score due to trip deleted");

        var state = GetState();
        SetState(state with { Score = null, JamTripCount = state.JamTripCount - 1 });

        return [];
    });

    public IEnumerable<Event> Handle(InitialTripCompleted @event) => @event.HandleIfTeam(teamSide, () =>
    {
        logger.LogDebug("Adding trip due to initial trip completed at tick {tick}", @event.Tick);
        SetState(GetState() with { Score = null, JamTripCount = @event.Body.TripCompleted ? 1 : 0 });

        return [];
    });

    public IEnumerable<Event> Handle(TripCompleted @event) => @event.HandleIfTeam(teamSide, () =>
    {
        logger.LogDebug("Adding trip due to trip completed at tick {tick}", @event.Tick);

        var state = GetState();
        SetState(state with { Score = null, JamTripCount = state.JamTripCount + 1 });

        return [];
    });

    public IEnumerable<Event> Tick(Tick tick)
    {
        var state = GetState();
        if (state.Score == null) return [];

        var jamClock = GetState<JamClockState>();
        if (!jamClock.IsRunning) return [];

        if (tick - state.LastScoreTick < TripScoreResetTimeInTicks) return [];

        logger.LogDebug("Resetting trip score due to trip time expiring at tick {tick}", tick);

        SetState(state with { Score = null, LastScoreTick = state.LastScoreTick + TripScoreResetTimeInTicks });

        return [new TripCompleted(tick, new(teamSide))];
    }
}

public sealed record TripScoreState(
    int? Score, 
    int JamTripCount, 
    [property: IgnoreChange] Tick LastScoreTick
);

public sealed class HomeTripScore(ReducerGameContext context, ILogger<HomeTripScore> logger) : TripScore(TeamSide.Home, context, logger);
public sealed class AwayTripScore(ReducerGameContext context, ILogger<AwayTripScore> logger) : TripScore(TeamSide.Away, context, logger);
