using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public sealed class LineupClock(GameContext gameContext, ILogger<LineupClock> logger)
    : Reducer<LineupClockState>(gameContext), IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , ITickReceiver
{
    protected override LineupClockState DefaultState => new(false, 0, 0);

    public void Handle(JamStarted @event)
    {
        if (GetState().IsRunning)
        {
            logger.LogDebug("Stopping lineup clock");
            SetState(GetState() with { IsRunning = false });
        }
    }

    public void Handle(JamEnded @event)
    {
        if (!GetState().IsRunning)
            SetState(new(true, @event.Tick, 0));
    }

    public void Tick(long tick, long tickDelta)
    {
        var state = GetState();

        if (!state.IsRunning) return;

        var newState = state with { TicksPassed = tick - state.StartTick };
        SetState(newState);
    }
}

public record LineupClockState(bool IsRunning, long StartTick, long TicksPassed);
