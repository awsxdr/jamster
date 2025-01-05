using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class IntermissionClock(ReducerGameContext context, ILogger<IntermissionClock> logger) 
    : Reducer<IntermissionClockState>(context)
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<IntermissionStarted>
    , IHandlesEvent<IntermissionClockSet>
    , IHandlesEvent<IntermissionEnded>
    , ITickReceiver
{
    protected override IntermissionClockState DefaultState => new(false, true, 0, 0);

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();

        if (!state.IsRunning) return [];

        logger.LogDebug("Stopping intermission clock due to jam start");

        SetState(state with { IsRunning = false });

        return [];
    }

    public IEnumerable<Event> Handle(IntermissionStarted @event)
    {
        logger.LogInformation("Intermission started");

        SetState(GetState() with { IsRunning = true });

        return [];
    }

    public IEnumerable<Event> Handle(IntermissionClockSet @event)
    {
        logger.LogDebug("Intermission length set to {length} seconds", @event.Body.SecondsRemaining);
        SetState(GetState() with
        {
            HasExpired = @event.Body.SecondsRemaining <= 0,
            SecondsRemaining = @event.Body.SecondsRemaining,
            TargetTick = @event.Tick + @event.Body.SecondsRemaining * 1000,
        });

        return [];
    }

    public IEnumerable<Event> Handle(IntermissionEnded @event)
    {
        var state = GetState();

        if (!state.IsRunning) return [];

        SetState(state with { IsRunning = false });

        return [];
    }

    public IEnumerable<Event> Tick(Tick tick)
    {
        var state = GetState();

        if (!state.IsRunning) return [];

        var ticksRemaining = (Tick) Math.Max(0, (long)state.TargetTick - (long)tick);

        SetState(state with
        {
            HasExpired = ticksRemaining == 0,
            SecondsRemaining = ticksRemaining.Seconds,
        });

        return [];
    }
}

public sealed record IntermissionClockState(
    bool IsRunning,
    bool HasExpired,
    Tick TargetTick,
    int SecondsRemaining);