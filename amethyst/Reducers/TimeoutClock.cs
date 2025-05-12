using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class TimeoutClock(ReducerGameContext context, ILogger<TimeoutClock> logger) 
    : Reducer<TimeoutClockState>(context)
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<TimeoutEnded>
    , IHandlesEvent<TimeoutClockSet>
    , IHandlesEvent<TimeoutTypeSet>
    , IHandlesEvent<PeriodEnded>
    , IHandlesEvent<PeriodFinalized>
    , ITickReceiver
    , IDependsOnState<RulesState>
    , IDependsOnState<TimeoutTypeState>
    , IDependsOnState<PeriodClockState>
{
    protected override TimeoutClockState DefaultState => new(false, 0, 0, TimeoutClockStopReason.None, 0);

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();

        if(state.IsRunning)
            SetState(state with { IsRunning = false });

        if (!state.IsRunning || state.EndTick > 0)
            return [];

        logger.LogDebug("Jam started; stopping timeout at {tick}", @event.Tick);

        return [new TimeoutEnded(@event.Tick)];
    }

    public IEnumerable<Event> Handle(TimeoutStarted @event)
    {
        logger.LogDebug("New timeout started at {tick}", @event.Tick);
        SetState(DefaultState with
        {
            IsRunning = true,
            StartTick = @event.Tick,
            EndTick = 0,
            StopReason = TimeoutClockStopReason.None,
        });

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutEnded @event)
    {
        var state = GetState();

        if (state is { EndTick: 0 })
        {
            logger.LogDebug("Ending timeout at {tick}", @event.Tick);
            SetState(state with
            {
                EndTick = @event.Tick,
                StopReason = TimeoutClockStopReason.Other,
            });
        }
        else
        {
            logger.LogDebug("Timeout ended but timeout has already ended at {tick}", @event.Tick);
        }

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutClockSet @event)
    {
        var state = GetState();

        var ticksPassed = Domain.Tick.FromSeconds(@event.Body.SecondsPassed);

        SetState(state with
        {
            StartTick = @event.Tick - ticksPassed,
            TicksPassed = ticksPassed,
        });

        return [];
    }

    public IEnumerable<Event> Handle(PeriodEnded @event)
    {
        var state = GetState();

        if (state is { IsRunning: false, EndTick: > 0 }) return [];

        logger.LogDebug("Stopping timeout clock due to period end");

        var timeoutAlreadyEnded = state.EndTick > 0;

        if (!timeoutAlreadyEnded)
        {
            SetState(GetState() with
            {
                IsRunning = false,
                EndTick = @event.Tick,
                StopReason = TimeoutClockStopReason.PeriodExpired,
                TicksPassed = @event.Tick - state.StartTick,
            });
        }

        return [];
    }

    public IEnumerable<Event> Handle(PeriodFinalized @event)
    {
        var state = GetState();

        if (!state.IsRunning) return [];

        logger.LogDebug("Stopping timeout clock due to period being finalized");

        var timeoutAlreadyEnded = state.EndTick > 0;

        if (timeoutAlreadyEnded)
        {
            SetState(GetState() with
            {
                IsRunning = false,
                StopReason = TimeoutClockStopReason.PeriodFinalized,
                TicksPassed = @event.Tick - state.StartTick,
            });
        }
        else
        {
            SetState(GetState() with
            {
                IsRunning = false,
                EndTick = @event.Tick,
                StopReason = TimeoutClockStopReason.PeriodFinalized,
                TicksPassed = @event.Tick - state.StartTick,
            });
        }

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutTypeSet @event)
    {
        var state = GetState();

        // We only care about this if the timeout stopped because it was running at the end of the period
        if (state.StopReason is not TimeoutClockStopReason.PeriodExpired and not TimeoutClockStopReason.None)
            return [];

        var rules = GetState<RulesState>().Rules;
        var stopTimeoutTypes = rules.TimeoutRules.PeriodClockBehavior;

        // If stopping for all timeout types then we don't care about the type being set
        if (stopTimeoutTypes == TimeoutPeriodClockStopBehavior.All)
            return [];

        var periodClock = GetCachedState<PeriodClockState>();

        var currentTimeoutType = GetCachedState<TimeoutTypeState>().TimeoutType;

        var currentTimeoutTypeStoppedClock =
            stopTimeoutTypes.HasFlag(TimeoutPeriodClockStopBehavior.TeamTimeout) && currentTimeoutType is CompoundTimeoutType.HomeTeamTimeout or CompoundTimeoutType.AwayTeamTimeout
            || stopTimeoutTypes.HasFlag(TimeoutPeriodClockStopBehavior.OfficialReview) && currentTimeoutType is CompoundTimeoutType.HomeOfficialReview or CompoundTimeoutType.AwayOfficialReview
            || stopTimeoutTypes.HasFlag(TimeoutPeriodClockStopBehavior.OfficialTimeout) && currentTimeoutType is CompoundTimeoutType.OfficialTimeout;

        var newTimeoutTypeStopsClock =
            stopTimeoutTypes.HasFlag(TimeoutPeriodClockStopBehavior.TeamTimeout) && @event.Body.Type == TimeoutType.Team
            || stopTimeoutTypes.HasFlag(TimeoutPeriodClockStopBehavior.OfficialReview) && @event.Body.Type == TimeoutType.Review
            || stopTimeoutTypes.HasFlag(TimeoutPeriodClockStopBehavior.OfficialTimeout) && @event.Body.Type == TimeoutType.Official;

        if (currentTimeoutTypeStoppedClock && !newTimeoutTypeStopsClock)
        {
            SetState(state with
            {
                IsRunning = !periodClock.HasExpired,
                EndTick = periodClock.HasExpired ? @event.Tick : state.EndTick,
                StopReason = periodClock.HasExpired ? TimeoutClockStopReason.PeriodExpired : TimeoutClockStopReason.None,
                TicksPassed = @event.Tick - state.StartTick,
            });
        }
        else if(!currentTimeoutTypeStoppedClock && newTimeoutTypeStopsClock)
        {
            SetState(state with
            {
                IsRunning = true,
                EndTick = 0,
                StopReason = TimeoutClockStopReason.None,
                TicksPassed = @event.Tick - state.StartTick,
            });
        }

        return [];
    }

    public IEnumerable<Event> Tick(Tick tick)
    {
        var state = GetState();

        if (!state.IsRunning) return [];

        var newState = state with
        {
            TicksPassed = tick - state.StartTick,
        };
        SetState(newState);

        return [];
    }
}

public record TimeoutClockState(
    bool IsRunning,
    long StartTick,
    long EndTick,
    TimeoutClockStopReason StopReason,
    [property: IgnoreChange] Tick TicksPassed)
{
    public int SecondsPassed => TicksPassed.Seconds;
}

public enum TimeoutClockStopReason
{
    None = 0,
    PeriodExpired,
    PeriodFinalized,
    Other,
}