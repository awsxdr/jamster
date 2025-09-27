using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public sealed class JamClock(ReducerGameContext gameContext, ILogger<JamClock> logger) 
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
    protected override JamClockState DefaultState => new(false, 0, 0, true, false);

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();
        RulesState rules = GetState<RulesState>();

        if (state.IsRunning && state.TicksPassed < Domain.Tick.FromSeconds(rules.Rules.JamRules.DurationInSeconds))
            return [];

        logger.LogDebug("Starting jam clock at {tick}", @event.Tick);

        SetState(state with
        {
            IsRunning = true,
            StartTick = @event.Tick,
            TicksPassed = 0,
            Expired = false,
        });

        return [];
    }

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        var state = GetState();

        if (!state.IsRunning)
            return [];

        logger.LogDebug("Stopping jam clock at {tick} due to jam end", @event.Tick);

        SetState(state with
        {
            IsRunning = false,
            AutoExpire = true,
            TicksPassed = @event.Tick - state.StartTick,
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
        };

        SetState(newState);
        var rules = GetState<RulesState>();

        if (ticksPassed < Domain.Tick.FromSeconds(rules.Rules.JamRules.DurationInSeconds)) return [];

        if (!state.AutoExpire)
        {
            return [];
        }

        logger.LogDebug("Jam clock expired, ending jam");

        SetState(GetState() with { Expired = true });

        return [new JamExpired(state.StartTick + Domain.Tick.FromSeconds(rules.Rules.JamRules.DurationInSeconds))];
    }
}

public record JamClockState(
    bool IsRunning,
    Tick StartTick,
    [property: IgnoreChange] Tick TicksPassed,
    bool AutoExpire,
    bool Expired)
{
    public int SecondsPassed => TicksPassed.Seconds;
}