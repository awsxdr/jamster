namespace amethyst.Reducers;

using Events;
using Services;

public class JamClock(IGameStateStore stateStore) 
    : Reducer<JamClockState>(stateStore)
        , IHandlesEvent<JamStarted>
        , IHandlesEvent<JamEnded>
{
    protected override JamClockState DefaultState => new(false, 0);

    public void Handle(JamStarted @event)
    {
        if(!GetState().IsRunning)
            SetState(new(true, @event.Tick));
    }

    public void Handle(JamEnded @event) =>
        SetState(GetState() with { IsRunning = false });
}

public record JamClockState(bool IsRunning, long StartTick);
public record LineupClockState(bool IsRunning, long StartTick);