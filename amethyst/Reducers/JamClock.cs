using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public sealed class JamClock(ReducerGameContext gameContext, IEventBus eventBus, ILogger<JamClock> logger) 
    : Reducer<JamClockState>(gameContext)
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<CallMarked>
    , IHandlesEvent<JamClockSet>
    , ITickReceiver
{
    protected override JamClockState DefaultState => new(false, 0, 0, 0);

    public const long JamLengthInTicks = 2 * 60 * 1000;

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();

        if (state.IsRunning)
            return [];

        logger.LogDebug("Starting jam clock");

        SetState(new(true, @event.Tick, 0, 0));

        return [];
    }

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        logger.LogDebug("Stopping jam clock due to jam end");

        SetState(GetState() with {IsRunning = false});

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutStarted @event)
    {
        var state = GetState();

        if(!state.IsRunning) return [];

        logger.LogDebug("Stopping jam clock due to timeout start");

        return [new JamEnded(@event.Tick)];
    }

    public IEnumerable<Event> Handle(CallMarked @event)
    {
        var state = GetState();

        if (!@event.Body.Call || !state.IsRunning) return [];

        return [new JamEnded(@event.Tick)];
    }

    public IEnumerable<Event> Handle(JamClockSet @event)
    {
        var state = GetState();

        var ticksRemaining = Domain.Tick.FromSeconds(@event.Body.SecondsRemaining);
        var ticksPassed = JamLengthInTicks - ticksRemaining;

        SetState(state with
        {
            StartTick = @event.Tick - ticksPassed,
            TicksPassed = ticksPassed,
            SecondsPassed = ticksPassed.Seconds,
        });

        return [];
    }

    public IEnumerable<Event> Tick(Tick tick)
    {
        var state = GetState();

        if (!state.IsRunning) return [];

        var ticksPassed = tick - state.StartTick;
        var newState = GetState() with
        {
            TicksPassed = ticksPassed,
            SecondsPassed = (int) (ticksPassed / 1000L)
        };

        SetState(newState);

        if (ticksPassed <= JamLengthInTicks) return [];

        logger.LogDebug("Jam clock expired, ending jam");

        _ = eventBus.AddEvent(Context.GameInfo, new JamEnded(Guid7.FromTick(state.StartTick + JamLengthInTicks)));

        return [];

    }
}

public record JamClockState(bool IsRunning, long StartTick, [property: IgnoreChange] long TicksPassed, int SecondsPassed);