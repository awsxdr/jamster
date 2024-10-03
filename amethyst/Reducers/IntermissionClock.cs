using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class IntermissionClock(GameContext context, ILogger<IntermissionClock> logger) 
    : Reducer<IntermissionClockState>(context)
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<IntermissionEnded>
    , ITickReceiver
{
    protected override IntermissionClockState DefaultState => new(false, true, 0, 0);

    public void Handle(JamStarted @event)
    {
        var state = GetState();

        if (!state.IsRunning) return;

        SetState(state with { IsRunning = false });
    }

    public void Handle(IntermissionEnded @event)
    {
        var state = GetState();

        if (!state.IsRunning) return;

        SetState(state with { IsRunning = false });
    }

    public void Tick(Tick tick, long tickDelta)
    {
        var state = GetState();

        if (!state.IsRunning) return;

        var ticksRemaining = Math.Max(0, state.TargetTick - tick);

        SetState(state with
        {
            HasExpired = ticksRemaining == 0,
            SecondsRemaining = (int)(ticksRemaining / 1000),
        });
    }
}

public sealed record IntermissionClockState(
    bool IsRunning,
    bool HasExpired,
    Tick TargetTick,
    int SecondsRemaining);