using amethyst.Events;
using amethyst.Services;
using System.Security;

namespace amethyst.Reducers;

public class GameStage(GameContext context) 
    : Reducer<GameStageState>(context)
    , IHandlesEvent<IntermissionEnded>
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<PeriodEnded>
{
    protected override GameStageState DefaultState => new(Stage.BeforeGame, 0, 0);

    public void Handle(IntermissionEnded @event)
    {
        var state = GetState();
        var newState = state.Stage switch
        {
            Stage.BeforeGame or Stage.Intermission => state with { Stage = Stage.Lineup, PeriodNumber = state.PeriodNumber + 1 },
            _ => state
        };

        if (newState != state)
        {
            SetState(newState);
        }
    }

    public void Handle(JamStarted @event)
    {
        var state = GetState();
        var newState = state.Stage switch
        {
            Stage.BeforeGame or Stage.Intermission => new GameStageState(Stage: Stage.Jam, JamNumber: 1, PeriodNumber: state.PeriodNumber + 1),
            Stage.Jam => state,
            _ => state with { Stage = Stage.Jam, JamNumber = state.JamNumber + 1}
        };

        if (newState != state)
        {
            SetState(newState);
        }
    }

    public void Handle(JamEnded @event)
    {
        var state = GetState();
        var periodClock = GetState<PeriodClockState>();
        var newState = state switch
        {
            (Stage.Jam, _, _) when periodClock.IsRunning => state with {Stage = Stage.Lineup},
            (Stage.Jam, 1, _) when !periodClock.IsRunning => state with {Stage = Stage.Intermission},
            (Stage.Jam, 2, _) when !periodClock.IsRunning => state with {Stage = Stage.AfterGame},
            _ => state
        };

        if (newState != state)
        {
            SetState(newState);
        }
    }

    public void Handle(TimeoutStarted @event)
    {
        var state = GetState();
        var newState = state.Stage switch
        {
            Stage.BeforeGame => state,
            _ => state with {Stage = Stage.Timeout}
        };

        if (newState != state)
        {
            SetState(newState);
        }
    }

    public void Handle(PeriodEnded @event)
    {
        var state = GetState();
        var newState = state switch
        {
            (Stage.BeforeGame or Stage.Intermission or Stage.AfterGame, _, _) => state,
            (_, 1, _) => state with {Stage = Stage.Intermission},
            (_, 2, _) => state with {Stage = Stage.AfterGame},
            _ => state
        };

        if (newState != state)
        {
            SetState(newState);
        }
    }
}

public record GameStageState(Stage Stage, int PeriodNumber, int JamNumber);

public enum Stage
{
    BeforeGame,
    Lineup,
    Jam,
    Timeout,
    Intermission,
    AfterGame,
}