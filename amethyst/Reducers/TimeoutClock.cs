using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class TimeoutClock(GameContext context, ILogger<TimeoutClock> logger) : Reducer<TimeoutClockState>(context),
    IHandlesEvent<JamStarted>
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<TimeoutEnded>
    , ITickReceiver
{
    protected override TimeoutClockState DefaultState => new(false, 0, 0, 0);

    public void Handle(JamStarted @event)
    {
        var state = GetState();
        if (state.IsRunning)
        {
            logger.LogDebug("Jam started; stopping timeout at {tick}", @event.Tick);
            var endTick = state.EndTick > 0 ? state.EndTick : @event.Tick;
            SetState(state with
            {
                IsRunning = false, 
                EndTick = endTick,
                TicksPassed = endTick - state.StartTick,
            });
        }
    }

    public void Handle(TimeoutStarted @event)
    {
        logger.LogDebug("New timeout started at {tick}", @event.Tick);
        SetState(DefaultState with { IsRunning = true, StartTick = @event.Tick });
    }

    public void Handle(TimeoutEnded @event)
    {
        var state = GetState();

        if (state is { IsRunning: true, EndTick: 0 })
        {
            logger.LogDebug("Ending timeout at {tick}", @event.Tick);
            SetState(state with { EndTick = @event.Tick });
        }
        else
        {
            logger.LogDebug("Timeout ended but timeout has already ended at {tick}", @event.Tick);
        }
    }

    public void Tick(long tick, long tickDelta)
    {
        var state = GetState();

        if (!state.IsRunning) return;

        var newState = state with { TicksPassed = tick - state.StartTick };
        SetState(newState);
    }
}

public record TimeoutClockState(bool IsRunning, long StartTick, long EndTick, long TicksPassed);