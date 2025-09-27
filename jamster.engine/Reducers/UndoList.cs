﻿using jamster.engine.Events;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public class UndoList(ReducerGameContext context) : Reducer<UndoListState>(context), IHandlesAllEvents
{
    protected override UndoListState DefaultState => new(null, null);

    public IEnumerable<Event> Handle(Event @event, Guid7? sourceEventId)
    {
        if (@event is not IShownInUndo)
            return [];

        if (sourceEventId?.Equals((Guid7)GameClock.TickEventId) ?? false)
            sourceEventId = null;

        SetState(new(sourceEventId ?? @event.Id, @event.GetType().Name));

        return [];
    }
}

public record UndoListState(Guid7? LatestUndoEventId, string? LatestUndoEventName);