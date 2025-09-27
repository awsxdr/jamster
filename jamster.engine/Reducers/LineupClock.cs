﻿using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public sealed class LineupClock(ReducerGameContext gameContext, ILogger<LineupClock> logger)
    : Reducer<LineupClockState>(gameContext), IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<PeriodEnded>
    , IHandlesEvent<LineupClockSet>
    , IHandlesEvent<IntermissionEnded>
    , IDependsOnState<JamClockState>
    , IDependsOnState<TimeoutClockState>
    , ITickReceiver
{
    protected override LineupClockState DefaultState => new(false, 0, 0);

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        if (!GetState().IsRunning) return [];

        logger.LogDebug("Jam started, stopping lineup clock");
        SetState(GetState() with { IsRunning = false });

        return [];
    }

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        if (GetState().IsRunning || GetState<TimeoutClockState>().IsRunning) return [];

        logger.LogDebug("Starting lineup clock due to jam end");
        SetState(new(true, @event.Tick, 0));

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutStarted @event)
    {
        if (!GetState().IsRunning) return [];

        logger.LogDebug("Timeout started, stopping lineup clock");
        SetState(GetState() with { IsRunning = false });

        return [];
    }

    public IEnumerable<Event> Handle(PeriodEnded @event)
    {
        if (!GetState().IsRunning) return [];

        logger.LogDebug("Stopping lineup clock due to period end");
        SetState(GetState() with { IsRunning = false });

        return [];
    }

    public IEnumerable<Event> Handle(LineupClockSet @event)
    {
        var state = GetState();

        var ticksPassed = Domain.Tick.FromSeconds(@event.Body.SecondsPassed);

        SetState(state with
        {
            StartTick = @event.Tick - ticksPassed,
            TicksPassed = ticksPassed,
        });

        return [];
    }

    public IEnumerable<Event> Handle(IntermissionEnded @event)
    {
        if(!GetState<JamClockState>().IsRunning)
            SetState(new LineupClockState(true, @event.Tick, 0));

        return [];
    }

    public IEnumerable<Event> Tick(Tick tick)
    {
        var state = GetState();

        if (!state.IsRunning) return [];

        var newState = state with
        {
            TicksPassed = tick - state.StartTick,
        };
        SetState(newState);

        return [];
    }
}

public record LineupClockState(bool IsRunning, Tick StartTick, [property: IgnoreChange] Tick TicksPassed)
{
    public int SecondsPassed => TicksPassed.Seconds;

}
