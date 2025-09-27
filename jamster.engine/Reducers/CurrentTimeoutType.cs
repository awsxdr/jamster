﻿using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public class CurrentTimeoutType(ReducerGameContext context) 
    : Reducer<CurrentTimeoutTypeState>(context)
    , IHandlesEvent<TimeoutTypeSet>
    , IHandlesEvent<TimeoutStarted>
{
    protected override CurrentTimeoutTypeState DefaultState => new(TimeoutType.Untyped, null);

    public IEnumerable<Event> Handle(TimeoutTypeSet @event)
    {
        SetStateIfDifferent(new(@event.Body.Type, @event.Body.TeamSide));

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutStarted @event)
    {
        SetStateIfDifferent(new(TimeoutType.Untyped, null));

        return [];
    }
}

public sealed record CurrentTimeoutTypeState(TimeoutType Type, TeamSide? TeamSide);