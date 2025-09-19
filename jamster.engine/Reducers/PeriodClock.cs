using jamster.Domain;
using jamster.Events;
using jamster.Services;

namespace jamster.Reducers;

public class PeriodClock(ReducerGameContext context, ILogger<PeriodClock> logger) 
    : Reducer<PeriodClockState>(context)
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<TimeoutEnded>
    , IHandlesEvent<TimeoutTypeSet>
    , IHandlesEvent<PeriodEnded>
    , IHandlesEvent<PeriodFinalized>
    , IHandlesEvent<PeriodClockSet>
    , IDependsOnState<JamClockState>
    , IDependsOnState<TimeoutTypeState>
    , IDependsOnState<RulesState>
    , ITickReceiver
{
    protected override PeriodClockState DefaultState => new(false, true, 0, 0, 0);

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();
        if (state.IsRunning) return [];

        logger.LogDebug("Starting period clock due to jam start");

        var rules = GetState<RulesState>().Rules;

        var ticksPassed = state.TicksPassed;
        var limitedTicksPassed = rules.PeriodRules.PeriodEndBehavior switch
        {
            PeriodEndBehavior.Manual => ticksPassed,
            _ => (Tick)Math.Min(Domain.Tick.FromSeconds(rules.PeriodRules.DurationInSeconds), ticksPassed)
        };
        var roundedTicksPassed = Domain.Tick.FromSeconds(limitedTicksPassed.Seconds);

        SetState(GetState() with
        {
            IsRunning = true, 
            HasExpired = false,
            TicksPassedAtLastStart = roundedTicksPassed,
            TicksPassed = roundedTicksPassed,
            LastStartTick = @event.Tick,
        });

        return [];
    }

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        var state = GetState();
        if (!state.IsRunning) return [];

        var ticksPassed = @event.Tick - state.LastStartTick + state.TicksPassedAtLastStart;

        var rules = GetState<RulesState>().Rules;

        if (ticksPassed < Domain.Tick.FromSeconds(rules.PeriodRules.DurationInSeconds)) return [];

        logger.LogInformation("Period clock expired following jam end");

        var limitedTicksPassed = rules.PeriodRules.PeriodEndBehavior switch
        {
            PeriodEndBehavior.Manual => ticksPassed,
            _ => (Tick)Math.Min(Domain.Tick.FromSeconds(rules.PeriodRules.DurationInSeconds), ticksPassed)
        };

        SetState(state with
        {
            IsRunning = false,
            HasExpired = true,
            TicksPassed = limitedTicksPassed
        });

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

        var ticksPassed = Math.Min(Domain.Tick.FromSeconds(rules.Rules.PeriodRules.DurationInSeconds), @event.Tick - state.LastStartTick + state.TicksPassedAtLastStart);

        SetState(state with
        {
            IsRunning = false,
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
            logger.LogDebug("Stopping period clock due to new timeout type");
            var newTicksPassed = state.TicksPassedAtLastStart + currentTimeoutStart - state.LastStartTick;
            SetState(state with
            {
                IsRunning = false,
                TicksPassed = newTicksPassed,
                HasExpired = newTicksPassed > Domain.Tick.FromSeconds(rules.PeriodRules.DurationInSeconds)
            });
        } 
        else if (!state.IsRunning && currentTimeoutTypeStoppedClock && !newTimeoutTypeStopsClock)
        {
            logger.LogDebug("Resuming period clock due to new timeout type");
            var newTicksPassed = state.TicksPassedAtLastStart + @event.Tick - state.LastStartTick;
            SetState(state with
            {
                IsRunning = true,
                TicksPassed = newTicksPassed,
                HasExpired = newTicksPassed > Domain.Tick.FromSeconds(rules.PeriodRules.DurationInSeconds)
            });
        }
        else if (!state.IsRunning && !currentTimeoutTypeStoppedClock && newTimeoutTypeStopsClock)
        {
            var newTicksPassed = state.TicksPassedAtLastStart + currentTimeoutStart - state.LastStartTick;
            SetState(state with
            {
                IsRunning = false,
                TicksPassed = newTicksPassed,
                HasExpired = newTicksPassed > Domain.Tick.FromSeconds(rules.PeriodRules.DurationInSeconds)
            });
        }

        state = GetState();
        if (state.HasExpired)
            return [new PeriodEnded(state.LastStartTick + Domain.Tick.FromSeconds(rules.PeriodRules.DurationInSeconds) - state.TicksPassedAtLastStart)];

        return [];
    }

    public IEnumerable<Event> Handle(PeriodEnded @event)
    {
        var state = GetState();

        if (state is { IsRunning: false, HasExpired: true })
            return [];

        logger.LogInformation("Stopping period clock due to period ended");

        var rules = GetState<RulesState>().Rules;

        var ticksPassed = @event.Tick - state.LastStartTick + state.TicksPassedAtLastStart;
        var limitedTicksPassed = rules.PeriodRules.PeriodEndBehavior switch
        {
            PeriodEndBehavior.Manual => ticksPassed,
            _ => (Tick)Math.Min(Domain.Tick.FromSeconds(rules.PeriodRules.DurationInSeconds), ticksPassed)
        };

        SetState(state with
        {
            IsRunning = false,
            HasExpired = true,
            TicksPassed = limitedTicksPassed,
        });

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
        var ticksPassed = Domain.Tick.FromSeconds(rules.Rules.PeriodRules.DurationInSeconds) - ticksRemaining;

        SetState(state with
        {
            LastStartTick = @event.Tick,
            TicksPassedAtLastStart = ticksPassed,
            TicksPassed = ticksPassed,
            HasExpired = ticksRemaining <= 0,
        });

        return [];
    }

    public IEnumerable<Event> Tick(Tick tick)
    {
        var state = GetState();
        if (!state.IsRunning) return [];

        var ticksPassed = (long)tick - state.LastStartTick + state.TicksPassedAtLastStart;

        var rules = GetState<RulesState>().Rules;

        if (!state.HasExpired && ticksPassed >= Domain.Tick.FromSeconds(rules.PeriodRules.DurationInSeconds))
        {
            logger.LogDebug("Period clock expired");

            if (rules.PeriodRules.PeriodEndBehavior == PeriodEndBehavior.Immediately)
            {
                logger.LogDebug("Ending period immediately due to ruleset");
                state = state with { IsRunning = false };
            }

            if (rules.PeriodRules.PeriodEndBehavior == PeriodEndBehavior.AnytimeOutsideJam && !GetState<JamClockState>().IsRunning)
            {
                logger.LogDebug("Period clock stopped due to expiry outside of jam");
                state = state with {IsRunning = false};
            }

            state = state with {HasExpired = true};
        }

        var limitedTicksPassed = rules.PeriodRules.PeriodEndBehavior switch
        {
            PeriodEndBehavior.Manual => ticksPassed,
            _ => (Tick)Math.Min(Domain.Tick.FromSeconds(rules.PeriodRules.DurationInSeconds), ticksPassed)
        };

        SetState(state with
        {
            TicksPassed = limitedTicksPassed,
        });

        if (!state.IsRunning)
        {
            return [new PeriodEnded(tick - (ticksPassed - Domain.Tick.FromSeconds(rules.PeriodRules.DurationInSeconds)))];
        }

        return [];
    }
}

public record PeriodClockState(
    bool IsRunning,
    bool HasExpired,
    Tick LastStartTick,
    Tick TicksPassedAtLastStart,
    [property: IgnoreChange] Tick TicksPassed)
{
    public int SecondsPassed => TicksPassed.Seconds;
}