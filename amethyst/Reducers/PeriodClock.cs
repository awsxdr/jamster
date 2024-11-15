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
    , ITickReceiver
{
    protected override PeriodClockState DefaultState => new(false, false, 0, 0, 0, 0);

    public static readonly Tick PeriodLengthInTicks = 30 * 60 * 1000;

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();
        if (state.IsRunning) return [];

        logger.LogDebug("Starting period clock due to jam start");

        SetState(GetState() with
        {
            IsRunning = true, 
            HasExpired = false,
            TicksPassedAtLastStart = state.TicksPassed,
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

        var ticksRemaining = PeriodLengthInTicks - state.TicksPassed;

        if (ticksRemaining > LineupClock.LineupDurationInTicks) return [];

        var lineupClock = GetState<LineupClockState>();
        var ticksRemainingWhenLastLineupStarted =
            PeriodLengthInTicks - lineupClock.StartTick - state.LastStartTick + state.TicksPassedAtLastStart;

        if (ticksRemainingWhenLastLineupStarted > LineupClock.LineupDurationInTicks) return [];

        logger.LogDebug("Starting period clock as timeout ended and previous lineup started with less than lineup duration on clock");

        SetState(state with
        {
            IsRunning = true,
            TicksPassedAtLastStart = state.TicksPassed,
            LastStartTick = @event.Tick,
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

    public IEnumerable<Event> Tick(Tick tick)
    {
        var state = GetState();
        if (!state.IsRunning) return [];

        var ticksPassed = Math.Min(PeriodLengthInTicks, (long)tick - state.LastStartTick + state.TicksPassedAtLastStart);

        if (!state.HasExpired && ticksPassed == PeriodLengthInTicks)
        {
            logger.LogDebug("Period clock expired");

            if (!GetState<JamClockState>().IsRunning)
            {
                logger.LogDebug("Period clock stopped due to expiry outside of jam");
                state = state with {IsRunning = false};
            }

            state = state with {HasExpired = true};
        }

        SetState(state with
        {
            TicksPassed = ticksPassed,
            SecondsPassed = (int)(ticksPassed / 1000L)
        });

        if (!state.IsRunning)
        {
            return [new PeriodEnded(tick)];
        }

        return [];
    }
}

public record PeriodClockState(bool IsRunning, bool HasExpired, long LastStartTick, long TicksPassedAtLastStart, [property: IgnoreChange] long TicksPassed, int SecondsPassed);