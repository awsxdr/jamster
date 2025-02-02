using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class PeriodClock(ReducerGameContext context, ILogger<PeriodClock> logger) 
    : Reducer<PeriodClockState>(context)
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<TimeoutEnded>
    , IHandlesEvent<TimeoutTypeSet>
    , IHandlesEvent<PeriodFinalized>
    , IHandlesEvent<PeriodClockSet>
    , IDependsOnState<JamClockState>
    , IDependsOnState<LineupClockState>
    , IDependsOnState<TimeoutTypeState>
    , IDependsOnState<RulesState>
    , ITickReceiver
{
    protected override PeriodClockState DefaultState => new(false, true, 0, 0, 0, 0);

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();
        if (state.IsRunning) return [];

        logger.LogDebug("Starting period clock due to jam start");

        SetState(GetState() with
        {
            IsRunning = true, 
            HasExpired = false,
            TicksPassedAtLastStart = (int)(Math.Floor(state.TicksPassed / 1000.0) * 1000),
            TicksPassed = (int)(Math.Floor(state.TicksPassed / 1000.0) * 1000),
            LastStartTick = @event.Tick,
        });

        return [];
    }

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        var state = GetState();
        if (!state.IsRunning) return [];

        Tick ticksPassed = @event.Tick - state.LastStartTick + state.TicksPassedAtLastStart;

        var rules = GetState<RulesState>();

        if (ticksPassed < rules.Rules.PeriodRules.Duration) return [];

        logger.LogInformation("Period clock expired following jam end");

        SetState(state with {IsRunning = false, HasExpired = true, SecondsPassed = ticksPassed.Seconds, TicksPassed = ticksPassed});

        return [new PeriodEnded(@event.Tick)];
    }

    public IEnumerable<Event> Handle(TimeoutStarted @event)
    {
        var state = GetState();
        if (!state.IsRunning) return [];

        var rules = GetState<RulesState>();
        var shouldStop = rules.Rules.TimeoutRules.PeriodClockBehavior == TimeoutPeriodClockStopBehavior.All;

        if (!shouldStop)
            return [];

        logger.LogDebug("Stopping period clock due to timeout start");

        var ticksPassed = Math.Min(rules.Rules.PeriodRules.Duration, @event.Tick - state.LastStartTick + state.TicksPassedAtLastStart);

        SetState(state with
        {
            IsRunning = false,
            SecondsPassed = (int)(ticksPassed / 1000),
            TicksPassed = ticksPassed,
        });

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutEnded @event)
    {
        var state = GetState();

        if (!state.HasExpired) return [];

        logger.LogDebug("Ending period as timeout ended with no time on the period clock");

        return [new PeriodEnded(@event.Tick)];
    }

    public IEnumerable<Event> Handle(TimeoutTypeSet @event)
    {
        var rules = GetState<RulesState>().Rules;
        var stopTimeoutTypes = rules.TimeoutRules.PeriodClockBehavior;
        var state = GetState();

        // If stopping for all timeout types and clock already stopped then we don't care about the type being set
        if (stopTimeoutTypes == TimeoutPeriodClockStopBehavior.All && !state.IsRunning)
            return [];

        var (currentTimeoutType, currentTimeoutStart) = GetCachedState<TimeoutTypeState>();

        var currentTimeoutTypeStoppedClock =
            stopTimeoutTypes.HasFlag(TimeoutPeriodClockStopBehavior.TeamTimeout) && currentTimeoutType is CompoundTimeoutType.HomeTeamTimeout or CompoundTimeoutType.AwayTeamTimeout
            || stopTimeoutTypes.HasFlag(TimeoutPeriodClockStopBehavior.OfficialReview) && currentTimeoutType is CompoundTimeoutType.HomeOfficialReview or CompoundTimeoutType.AwayOfficialReview
            || stopTimeoutTypes.HasFlag(TimeoutPeriodClockStopBehavior.OfficialTimeout) && currentTimeoutType is CompoundTimeoutType.OfficialTimeout;

        var newTimeoutTypeStopsClock =
            stopTimeoutTypes == TimeoutPeriodClockStopBehavior.All
            || stopTimeoutTypes.HasFlag(TimeoutPeriodClockStopBehavior.TeamTimeout) && @event.Body.Type == TimeoutType.Team
            || stopTimeoutTypes.HasFlag(TimeoutPeriodClockStopBehavior.OfficialReview) && @event.Body.Type == TimeoutType.Review
            || stopTimeoutTypes.HasFlag(TimeoutPeriodClockStopBehavior.OfficialTimeout) && @event.Body.Type == TimeoutType.Official;

        if (state.IsRunning && newTimeoutTypeStopsClock)
        {
            var newTicksPassed = state.TicksPassedAtLastStart + currentTimeoutStart - state.LastStartTick;
            SetState(state with
            {
                IsRunning = false,
                TicksPassed = newTicksPassed,
                SecondsPassed = newTicksPassed.Seconds,
                HasExpired = newTicksPassed > rules.PeriodRules.Duration
            });
        } 
        else if (!state.IsRunning && currentTimeoutTypeStoppedClock && !newTimeoutTypeStopsClock)
        {
            var newTicksPassed = state.TicksPassedAtLastStart + @event.Tick - state.LastStartTick;
            SetState(state with
            {
                IsRunning = true,
                TicksPassed = newTicksPassed,
                SecondsPassed = newTicksPassed.Seconds,
                HasExpired = newTicksPassed > rules.PeriodRules.Duration
            });
        }

        return [];
    }

    public IEnumerable<Event> Handle(PeriodFinalized @event)
    {
        logger.LogInformation("Resetting period clock due to period finalization");

        if (GetState().IsRunning)
            return [new PeriodEnded(@event.Tick)];

        SetState(DefaultState);

        return [];
    }

    public IEnumerable<Event> Handle(PeriodClockSet @event)
    {
        var state = GetState();
        var rules = GetState<RulesState>();

        var ticksRemaining = Domain.Tick.FromSeconds(@event.Body.SecondsRemaining);
        var ticksPassed = rules.Rules.PeriodRules.Duration - ticksRemaining;

        SetState(state with
        {
            LastStartTick = @event.Tick,
            TicksPassedAtLastStart = ticksPassed,
            TicksPassed = ticksPassed,
            SecondsPassed = ticksPassed.Seconds,
            HasExpired = ticksRemaining <= 0,
        });

        return [];
    }

    public IEnumerable<Event> Tick(Tick tick)
    {
        var state = GetState();
        if (!state.IsRunning) return [];

        var ticksPassed = (long)tick - state.LastStartTick + state.TicksPassedAtLastStart;

        var rules = GetState<RulesState>();

        if (!state.HasExpired && ticksPassed >= rules.Rules.PeriodRules.Duration)
        {
            logger.LogDebug("Period clock expired");

            if (!GetState<JamClockState>().IsRunning)
            {
                logger.LogDebug("Period clock stopped due to expiry outside of jam");
                state = state with {IsRunning = false};
            }

            state = state with {HasExpired = true};
        }

        var limitedTicksPassed = Math.Min(rules.Rules.PeriodRules.Duration, ticksPassed);

        SetState(state with
        {
            TicksPassed = limitedTicksPassed,
            SecondsPassed = (int)(limitedTicksPassed / 1000L)
        });

        if (!state.IsRunning)
        {
            return [new PeriodEnded(tick - (ticksPassed - rules.Rules.PeriodRules.Duration))];
        }

        return [];
    }
}

public record PeriodClockState(bool IsRunning, bool HasExpired, long LastStartTick, Tick TicksPassedAtLastStart, [property: IgnoreChange] long TicksPassed, int SecondsPassed);