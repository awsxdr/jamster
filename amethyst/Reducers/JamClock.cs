namespace amethyst.Reducers;

using Events;
using Services;

public class JamClock(GameContext gameContext, IEventBus eventBus) 
    : Reducer<JamClockState>(gameContext)
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , ITickReceiver
{
    protected override JamClockState DefaultState => new(false, 0, 0);

    public void Handle(JamStarted @event)
    {
        if(!GetState().IsRunning)
            SetState(new(true, @event.Tick, 0));
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

public record JamClockState(bool IsRunning, long StartTick, long TicksPassed);
