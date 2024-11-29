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
    , IHandlesEvent<JamEnded>
    , IDependsOnState<JamClockState>
    , ITickReceiver
{
    protected override TripScoreState DefaultState => new(0, 0);

    public static readonly Tick TripScoreResetTimeInTicks = 3000;

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(ScoreModifiedRelative @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();
        var jamClock = GetState<JamClockState>();

        var keepCurrentValue = !jamClock.IsRunning || @event.Tick - state.LastChangeTick < TripScoreResetTimeInTicks;

        SetState(new(
            Math.Min(4, Math.Max(0,
                keepCurrentValue
                ? state.Score + @event.Body.Value
                : @event.Body.Value)),
            @event.Tick
        ));

        return [];
    });

    public IEnumerable<Event> Handle(ScoreSet @event) => @event.HandleIfTeam(teamSide, () =>
    {
        logger.LogDebug("Resetting trip score due to absolute score being set");

        SetState(new(0, @event.Tick));

        return [];
    });

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        logger.LogDebug("Resetting trip score due to jam end");

        SetState(new (0, @event.Tick));

        return [];
    }

    public IEnumerable<Event> Tick(Tick tick)
    {
        var state = GetState();
        if (state.Score == 0) return [];

        var jamClock = GetState<JamClockState>();
        if (!jamClock.IsRunning) return [];

        if (tick - state.LastChangeTick < TripScoreResetTimeInTicks) return [];

        logger.LogDebug("Resetting trip score due to trip time expiring");

        SetState(new (Score: 0, LastChangeTick: state.LastChangeTick + TripScoreResetTimeInTicks));

        return [];
    }
}

public sealed record TripScoreState(int Score, Tick LastChangeTick);

public sealed class HomeTripScore(ReducerGameContext context, ILogger<HomeTripScore> logger) : TripScore(TeamSide.Home, context, logger);
public sealed class AwayTripScore(ReducerGameContext context, ILogger<AwayTripScore> logger) : TripScore(TeamSide.Away, context, logger);
