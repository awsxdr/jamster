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
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<TimeoutEnded>
    , ITickReceiver
    , IDependsOnState<PeriodClockState>
{
    protected override IntermissionClockState DefaultState => new(false, true, IntermissionDurationInTicks, 0, 0);

    public static readonly Tick IntermissionDurationInTicks = Domain.Tick.FromSeconds(15 * 60);

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

        var state = GetState();

        SetState(state with
        {
            IsRunning = true,
            HasExpired = false,
            SecondsRemaining = state.TargetTick == 0 ? state.InitialDurationTicks.Seconds : state.SecondsRemaining,
            TargetTick = state.TargetTick == 0 ? @event.Tick + state.InitialDurationTicks : @event.Tick + Domain.Tick.FromSeconds(state.SecondsRemaining),
        });

        return [];
    }

    public IEnumerable<Event> Handle(IntermissionClockSet @event)
    {
        logger.LogDebug("Intermission length set to {length} seconds", @event.Body.SecondsRemaining);

        var periodClock = GetState<PeriodClockState>();

        SetState(GetState() with
        {
            HasExpired = @event.Body.SecondsRemaining <= 0,
            SecondsRemaining = @event.Body.SecondsRemaining,
            TargetTick = periodClock.HasExpired ? @event.Tick + @event.Body.SecondsRemaining * 1000 : 0,
            InitialDurationTicks = Domain.Tick.FromSeconds(@event.Body.SecondsRemaining),
        });

        return periodClock.HasExpired
            ? [new IntermissionStarted(@event.Tick)]
            : [];
    }

    public IEnumerable<Event> Handle(IntermissionEnded @event)
    {
        var state = GetState();

        if (!state.IsRunning) return [];

        SetState(state with { IsRunning = false });

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutStarted @event)
    {
        var state = GetState();

        if (!state.IsRunning) return [];

        logger.LogDebug("Stopping intermission clock due to timeout start");

        SetState(state with { IsRunning = false });

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutEnded @event)
    {
        var periodClock = GetState<PeriodClockState>();

        if (!periodClock.HasExpired) return [];

        logger.LogDebug("Restarting intermission clock after timeout");

        var state = GetState();
        SetState(state with
        {
            HasExpired = false,
            IsRunning = true,
            SecondsRemaining = state.InitialDurationTicks.Seconds,
            TargetTick = @event.Tick + state.InitialDurationTicks,
        });

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
    Tick InitialDurationTicks,
    Tick TargetTick,
    int SecondsRemaining);