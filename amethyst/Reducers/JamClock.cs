using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public sealed class JamClock(ReducerGameContext gameContext, IEventBus eventBus, ILogger<JamClock> logger) 
    : Reducer<JamClockState>(gameContext)
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<JamExpired>
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<CallMarked>
    , IHandlesEvent<JamClockSet>
    , IHandlesEvent<JamAutoExpiryDisabled>
    , IDependsOnState<RulesState>
    , ITickReceiver
{
    protected override JamClockState DefaultState => new(false, 0, 0, 0, true);

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();
        RulesState rules = GetState<RulesState>();

        if (state.IsRunning && state.TicksPassed < Domain.Tick.FromSeconds(rules.Rules.JamRules.DurationInSeconds))
            return [];

        logger.LogDebug("Starting jam clock");

        SetState(state with
        {
            IsRunning = true,
            StartTick = @event.Tick,
            TicksPassed = 0,
            SecondsPassed = 0,
        });

        return [];
    }

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        var state = GetState();

        if (!state.IsRunning)
            return [];

        logger.LogDebug("Stopping jam clock due to jam end");

        SetState(state with
        {
            IsRunning = false,
            AutoExpire = true,
            TicksPassed = @event.Tick - state.StartTick,
            SecondsPassed = ((Tick) @event.Tick - state.StartTick).Seconds,
        });

        return [];
    }

    public IEnumerable<Event> Handle(JamExpired @event) =>
        [new JamEnded(@event.Tick)];

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
        var rules = GetState<RulesState>();

        var ticksRemaining = Domain.Tick.FromSeconds(@event.Body.SecondsRemaining);
        var ticksPassed = Domain.Tick.FromSeconds(rules.Rules.JamRules.DurationInSeconds) - ticksRemaining;

        SetState(state with
        {
            StartTick = @event.Tick - ticksPassed,
            TicksPassed = ticksPassed,
            SecondsPassed = ticksPassed.Seconds,
        });

        return [];
    }

    public IEnumerable<Event> Handle(JamAutoExpiryDisabled @event)
    {
        SetState(GetState() with { AutoExpire = false });

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
        var rules = GetState<RulesState>();

        if (ticksPassed <= Domain.Tick.FromSeconds(rules.Rules.JamRules.DurationInSeconds)) return [];

        if (!state.AutoExpire)
        {
            logger.LogDebug("Jam clock expired but still running");
            return [];
        }

        logger.LogDebug("Jam clock expired, ending jam");

        return [new JamExpired(state.StartTick + Domain.Tick.FromSeconds(rules.Rules.JamRules.DurationInSeconds))];
    }
}

public record JamClockState(bool IsRunning, long StartTick, [property: IgnoreChange] long TicksPassed, int SecondsPassed, bool AutoExpire);