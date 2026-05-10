using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public class PostGameClock(ReducerGameContext context, ILogger<PostGameClock> logger)
    : Reducer<PostGameClockState>(context)
    , IHandlesEvent<PeriodEnded>
    , IHandlesEvent<PeriodFinalized>
    , IHandlesEvent<OvertimeStarted>
    , IHandlesEvent<OvertimeEnded>
    , IDependsOnState<GameStageState>
    , IDependsOnState<OvertimeState>
    , IDependsOnState<RulesState>
    , ITickReceiver
{
    protected override PostGameClockState DefaultState => PostGameClockState.Default;

    public IEnumerable<Event> Handle(PeriodEnded @event)
    {
        var gameStage = GetState<GameStageState>();
        var rules = GetState<RulesState>().Rules;

        var isLastPeriod = GetState<OvertimeState>().IsInOvertime || gameStage.PeriodNumber >= rules.PeriodRules.PeriodCount;

        if (!isLastPeriod)
            return [];

        logger.LogDebug("Starting post-game clock due to last period ending at {tick}", @event.Tick);

        SetState(GetState() with
        {
            IsRunning = true,
            StartTick = @event.Tick,
            EndTick = 0,
        });

        return [];
    }

    public IEnumerable<Event> Handle(PeriodFinalized @event)
    {
        var state = GetState();

        if (!state.IsRunning) return [];

        logger.LogDebug("Stopping post-game clock due to period finalized at {tick}", @event.Tick);

        SetState(state with
        {
            IsRunning = false,
            EndTick = @event.Tick,
            TicksPassed = @event.Tick - state.StartTick,
        });

        return [];
    }

    public IEnumerable<Event> Handle(OvertimeStarted @event)
    {
        var state = GetState();

        if (!state.IsRunning) return [];

        logger.LogDebug("Stopping post-game clock due to overtime starting at {tick}", @event.Tick);

        SetState(state with
        {
            IsRunning = false,
            EndTick = @event.Tick,
            TicksPassed = @event.Tick - state.StartTick,
        });

        return [];
    }

    public IEnumerable<Event> Handle(OvertimeEnded @event)
    {
        logger.LogDebug("Starting post-game clock due to overtime ending at {tick}", @event.Tick);

        SetState(GetState() with
        {
            IsRunning = true,
            StartTick = @event.Tick,
            EndTick = 0,
            TicksPassed = 0,
        });

        return [];
    }

    public IEnumerable<Event> Tick(Tick tick)
    {
        var state = GetState();

        if (!state.IsRunning) return [];

        SetState(state with { TicksPassed = tick - state.StartTick });

        return [];
    }
}

public record PostGameClockState(
    bool IsRunning,
    long StartTick,
    long EndTick,
    [property: IgnoreChange] Tick TicksPassed)
{
    public int SecondsPassed => TicksPassed.Seconds;

    public static PostGameClockState Default => new(false, 0, 0, 0);
}
