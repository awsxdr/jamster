using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class GameStage(ReducerGameContext context, ILogger<GameStage> logger) 
    : Reducer<GameStageState>(context)
    , IHandlesEvent<IntermissionEnded>
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<TimeoutEnded>
    , IHandlesEvent<TimeoutTypeSet>
    , IHandlesEvent<PeriodEnded>
    , IHandlesEvent<PeriodFinalized>
    , IDependsOnState<PeriodClockState>
    , IDependsOnState<RulesState>
    , IDependsOnState<TimeoutTypeState>
{
    protected override GameStageState DefaultState => new(Stage.BeforeGame, 1, 0, false);

    public IEnumerable<Event> Handle(IntermissionEnded @event)
    {
        var state = GetState();
        var newState = state.Stage switch
        {
            Stage.Intermission => state with { Stage = Stage.Lineup, PeriodNumber = state.PeriodNumber + 1 },
            Stage.BeforeGame => state with { Stage = Stage.Lineup },
            _ => state
        };

        if(SetStateIfDifferent(newState))
            logger.LogDebug("Setting game state to {state} after intermission end", newState);

        return [];
    }

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        var state = GetState();
        var rules = GetState<RulesState>().Rules;

        var newState = state.Stage switch
        {
            Stage.BeforeGame => state with { Stage = Stage.Jam, JamNumber = 1, PeriodNumber = 1 },
            Stage.Intermission => state with { Stage = Stage.Jam, JamNumber = rules.JamRules.ResetJamNumbersBetweenPeriods ? 1 : state.JamNumber + 1 },
            Stage.Jam => state,
            _ => state with { Stage = Stage.Jam, JamNumber = state.JamNumber + 1}
        } with { PeriodIsFinalized = false };

        if (SetStateIfDifferent(newState))
            logger.LogDebug("Setting game state to {state} after jam start", newState);

        return (state.Stage is Stage.BeforeGame or Stage.Intermission)
            ? [new IntermissionEnded(@event.Tick)]
            : [];
    }

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        var state = GetState();

        if (state.Stage != Stage.Jam)
            return [];

        var rules = GetState<RulesState>().Rules;
        var periodClock = GetState<PeriodClockState>();

        if (periodClock.HasExpired)
        {
            if (state.PeriodNumber >= rules.PeriodRules.PeriodCount)
            {
                logger.LogDebug("Setting game state to AfterGame after jam end");
                SetState(state with { Stage = Stage.AfterGame });
            }
            else
            {
                logger.LogDebug("Setting game state to Intermission after jam end");
                SetState(state with { Stage = Stage.Intermission });
                return [new IntermissionStarted(@event.Tick)];
            }
        }
        else
        {
            logger.LogDebug("Setting game state to Lineup after jam end");
            SetState(state with { Stage = Stage.Lineup });
        }

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutStarted @event)
    {
        var state = GetState();
        var newState = state.Stage switch
        {
            Stage.BeforeGame => state,
            _ => state with {Stage = Stage.Timeout}
        };

        if (SetStateIfDifferent(newState))
            logger.LogDebug("Setting game state to {state} after timeout start", newState);

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutEnded @event)
    {
        var state = GetState();
        var newState = state.Stage switch
        {
            Stage.Timeout => state with {Stage = Stage.AfterTimeout},
            _ => state
        };

        if (SetStateIfDifferent(newState))
            logger.LogDebug("Setting game state to {state} after timeout end", newState);

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutTypeSet @event)
    {
        var rules = GetState<RulesState>().Rules;
        var periodClockBehavior = rules.TimeoutRules.PeriodClockBehavior;

        if (periodClockBehavior == TimeoutPeriodClockStopBehavior.All)
            return [];

        var state = GetState();
        var periodClock = GetCachedState<PeriodClockState>();

        if (!periodClock.HasExpired || state.PeriodIsFinalized)
            return [];

        var timeoutStopsPeriodClock =
            @event.Body.Type is TimeoutType.Team && periodClockBehavior.HasFlag(TimeoutPeriodClockStopBehavior.TeamTimeout)
            || @event.Body.Type is TimeoutType.Review && periodClockBehavior.HasFlag(TimeoutPeriodClockStopBehavior.OfficialReview)
            || @event.Body.Type is TimeoutType.Official && periodClockBehavior.HasFlag(TimeoutPeriodClockStopBehavior.OfficialTimeout);

        SetState(state with
        {
            Stage = timeoutStopsPeriodClock ? Stage.Timeout : state.Stage,
        });

        return [];
    }

    public IEnumerable<Event> Handle(PeriodEnded @event)
    {
        var state = GetState();

        if (state.Stage is not Stage.Jam and not Stage.Lineup and not Stage.Timeout and not Stage.AfterTimeout)
            return [];

        var rules = GetState<RulesState>().Rules;
        GameStageState newState;

        if (state.PeriodNumber >= rules.PeriodRules.PeriodCount)
        {
            logger.LogDebug("Setting game state to AfterGame after period end");
            newState = state with { Stage = Stage.AfterGame };
        }
        else
        {
            logger.LogDebug("Setting game state to Intermission after period end");
            newState = state with { Stage = Stage.Intermission };
        }

        SetState(newState);

        if (newState.Stage != Stage.Intermission) return [];

        return [new IntermissionStarted(@event.Tick)];
    }

    public IEnumerable<Event> Handle(PeriodFinalized @event)
    {
        var state = GetState();
        var rules = GetState<RulesState>().Rules;

        var newState = state switch
        {
            (Stage.Intermission, _, _, false) => state with
            {
                JamNumber = rules.JamRules.ResetJamNumbersBetweenPeriods ? 0 : state.JamNumber,
                PeriodNumber = state.PeriodNumber + 1,
                PeriodIsFinalized = true,
            },
            (Stage.AfterGame, _, _, false) => state with {PeriodIsFinalized = true},
            _ => state
        };

        if (SetStateIfDifferent(newState))
            logger.LogDebug("Setting game state to {state} after period finalized", newState);

        return [];
    }
}

public record GameStageState(Stage Stage, int PeriodNumber, int JamNumber, bool PeriodIsFinalized);

public enum Stage
{
    BeforeGame,
    Lineup,
    Jam,
    Timeout,
    AfterTimeout,
    Intermission,
    AfterGame,
}