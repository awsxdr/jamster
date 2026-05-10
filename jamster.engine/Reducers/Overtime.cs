using jamster.engine.Events;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public sealed class Overtime(ReducerGameContext context)
    : Reducer<OvertimeState>(context)
    , IHandlesEvent<OvertimeStarted>
    , IHandlesEvent<OvertimeEnded>
{
    protected override OvertimeState DefaultState => new(false);

    public IEnumerable<Event> Handle(OvertimeStarted @event)
    {
        SetState(new(true));
        return [];
    }

    public IEnumerable<Event> Handle(OvertimeEnded @event)
    {
        SetState(new(false));
        return [];
    }
}

public sealed record OvertimeState(bool IsInOvertime);
