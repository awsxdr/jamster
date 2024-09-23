using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public sealed class JamClock(GameContext gameContext, IEventBus eventBus) 
    : Reducer<JamClockState>(gameContext)
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , ITickReceiver
{
    protected override JamClockState DefaultState => new(false, 0, 0, 0);

    public void Handle(JamStarted @event)
    {
        var state = GetState();

        if(!state.IsRunning)
            SetState(new(true, state.JamNumber + 1, @event.Tick, 0));
    }

    public void Handle(JamEnded @event) =>
        SetState(GetState() with { IsRunning = false });

    public void Tick(long tick, long tickDelta)
    {
        var state = GetState();

        if (!state.IsRunning) return;

        var newState = state with { TicksPassed = tick - state.StartTick };
        if (newState.TicksPassed > 2 * 60 * 1000)
        {
            eventBus.AddEvent(Context.GameInfo, new JamEnded(state.StartTick + 2 * 60 * 1000));
        }

        SetState(newState);
    }
}

public record JamClockState(bool IsRunning, int JamNumber, long StartTick, long TicksPassed);
