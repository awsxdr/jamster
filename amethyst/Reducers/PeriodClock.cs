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
    , IHandlesEvent<PeriodFinalized>
    , IHandlesEvent<PeriodClockSet>
    , IDependsOnState<JamClockState>
    , IDependsOnState<LineupClockState>
    , ITickReceiver
{
    protected override PeriodClockState DefaultState => new(false, true, 0, 0, 0, 0);

    public static readonly Tick PeriodLengthInTicks = Domain.Tick.FromSeconds(30 * 60);

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

        if (ticksPassed < PeriodLengthInTicks) return [];

        logger.LogInformation("Period clock expired following jam end");

        SetState(state with {IsRunning = false, HasExpired = true, SecondsPassed = ticksPassed.Seconds, TicksPassed = ticksPassed});

        return [new PeriodEnded(@event.Tick)];
    }

    public IEnumerable<Event> Handle(TimeoutStarted @event)
    {
        var state = GetState();
        if (!state.IsRunning) return [];

        logger.LogDebug("Stopping period clock due to timeout start");

        var ticksPassed = Math.Min(PeriodLengthInTicks, @event.Tick - state.LastStartTick + state.TicksPassedAtLastStart);

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

        var ticksRemaining = Domain.Tick.FromSeconds(@event.Body.SecondsRemaining);
        var ticksPassed = PeriodLengthInTicks - ticksRemaining;

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

        if (!state.HasExpired && ticksPassed >= PeriodLengthInTicks)
        {
            logger.LogDebug("Period clock expired");

            if (!GetState<JamClockState>().IsRunning)
            {
                logger.LogDebug("Period clock stopped due to expiry outside of jam");
                state = state with {IsRunning = false};
            }

            state = state with {HasExpired = true};
        }

        var limitedTicksPassed = Math.Min(PeriodLengthInTicks, ticksPassed);

        SetState(state with
        {
            TicksPassed = limitedTicksPassed,
            SecondsPassed = (int)(limitedTicksPassed / 1000L)
        });

        if (!state.IsRunning)
        {
            return [new PeriodEnded(tick - (ticksPassed - PeriodLengthInTicks))];
        }

        return [];
    }
}

public record PeriodClockState(bool IsRunning, bool HasExpired, long LastStartTick, long TicksPassedAtLastStart, [property: IgnoreChange] long TicksPassed, int SecondsPassed);