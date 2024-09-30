using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class PeriodClock(GameContext context, ILogger<PeriodClock> logger) 
    : Reducer<PeriodClockState>(context)
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<TimeoutEnded>
    , ITickReceiver
{
    protected override PeriodClockState DefaultState => new(false, 0, 0, 0, 0);

    private const long PeriodLengthInTicks = 30 * 60 * 1000;
    private const long LineupDurationInTicks = 30 * 1000;

    public void Handle(JamStarted @event)
    {
        var state = GetState();
        if (state.IsRunning) return;

        logger.LogDebug("Starting period clock due to jam start");

        SetState(GetState() with
        {
            IsRunning = true, 
            TicksPassedAtLastStart = state.TicksPassed,
            LastStartTick = @event.Tick,
        });
    }

    public void Handle(JamEnded @event)
    {
        var state = GetState();
        if (!state.IsRunning) return;

        var ticksPassed = @event.Tick - state.LastStartTick + state.TicksPassedAtLastStart;

        if (ticksPassed < PeriodLengthInTicks) return;

        logger.LogInformation("Period clock expired following jam end");

        SetState(state with {IsRunning = false, SecondsPassed = (int) (ticksPassed / 1000), TicksPassed = ticksPassed});
    }

    public void Handle(TimeoutStarted @event)
    {
        var state = GetState();
        if (!state.IsRunning) return;

        logger.LogDebug("Stopping period clock due to timeout start");

        var ticksPassed = Math.Min(PeriodLengthInTicks, @event.Tick - state.LastStartTick + state.TicksPassedAtLastStart);

        SetState(state with
        {
            IsRunning = false,
            SecondsPassed = (int)(ticksPassed / 1000),
            TicksPassed = ticksPassed,
        });
    }

    public void Handle(TimeoutEnded @event)
    {
        var state = GetState();

        var ticksRemaining = PeriodLengthInTicks - state.TicksPassed;

        if (ticksRemaining > LineupDurationInTicks) return;

        logger.LogDebug("Starting period clock as timeout ended with less than lineup duration on clock");

        SetState(state with
        {
            IsRunning = true,
            TicksPassedAtLastStart = state.TicksPassed,
            LastStartTick = @event.Tick,
        });
    }

    public void Tick(long tick, long tickDelta)
    {
        var state = GetState();
        if (!state.IsRunning) return;

        var ticksPassed = Math.Min(PeriodLengthInTicks, tick - state.LastStartTick + state.TicksPassedAtLastStart);

        if (ticksPassed == PeriodLengthInTicks)
        {
            logger.LogDebug("Period clock expired");
        }

        SetState(state with
        {
            TicksPassed = ticksPassed,
            SecondsPassed = (int)(ticksPassed / 1000L)
        });
    }
}

public record PeriodClockState(bool IsRunning, long LastStartTick, long TicksPassedAtLastStart, [property: IgnoreChange] long TicksPassed, int SecondsPassed);