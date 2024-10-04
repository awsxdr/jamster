using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class GameStage(GameContext context, IEventBus eventBus, ILogger<GameStage> logger) 
    : Reducer<GameStageState>(context)
    , IHandlesEvent<IntermissionEnded>
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEventAsync<PeriodEnded>
    , IHandlesEvent<PeriodFinalized>
{
    protected override GameStageState DefaultState => new(Stage.BeforeGame, 0, 0, false);

    public void Handle(IntermissionEnded @event)
    {
        var state = GetState();
        var newState = state.Stage switch
        {
            Stage.BeforeGame or Stage.Intermission => state with { Stage = Stage.Lineup, PeriodNumber = state.PeriodNumber + 1 },
            _ => state
        };

        if(SetStateIfDifferent(newState))
            logger.LogDebug("Setting game state to {state} after intermission end", newState);
    }

    public void Handle(JamStarted @event)
    {
        var state = GetState();

        var jamClock = GetCachedState<JamClockState>();

        var newState = state.Stage switch
        {
            Stage.BeforeGame or Stage.Intermission => state with { Stage = Stage.Jam, JamNumber = 1 },
            Stage.Jam when @event.Tick - jamClock.StartTick > JamClock.JamLengthInTicks => state with { JamNumber = state.JamNumber + 1 },
            Stage.Jam => state,
            _ => state with { Stage = Stage.Jam, JamNumber = state.JamNumber + 1}
        } with { PeriodIsFinalized = false };

        if (SetStateIfDifferent(newState))
            logger.LogDebug("Setting game state to {state} after jam start", newState);
    }

    public void Handle(JamEnded @event)
    {
        var state = GetState();
        var periodClock = GetState<PeriodClockState>();
        var newState = state switch
        {
            (Stage.Jam, _, _, _) when periodClock.IsRunning => state with {Stage = Stage.Lineup},
            (Stage.Jam, 1, _, _) when !periodClock.IsRunning => state with {Stage = Stage.Intermission},
            (Stage.Jam, 2, _, _) when !periodClock.IsRunning => state with {Stage = Stage.AfterGame},
            _ => state
        };

        if (SetStateIfDifferent(newState))
            logger.LogDebug("Setting game state to {state} after jam end", newState);
    }

    public void Handle(TimeoutStarted @event)
    {
        var state = GetState();
        var newState = state.Stage switch
        {
            Stage.BeforeGame => state,
            _ => state with {Stage = Stage.Timeout}
        };

        if (SetStateIfDifferent(newState))
            logger.LogDebug("Setting game state to {state} after timeout start", newState);
    }

    public async Task HandleAsync(PeriodEnded @event)
    {
        var state = GetState();
        var newState = state switch
        {
            (Stage.BeforeGame or Stage.Intermission or Stage.AfterGame, _, _, _) => state,
            (_, 1, _, _) => state with {Stage = Stage.Intermission},
            (_, 2, _, _) => state with {Stage = Stage.AfterGame},
            _ => state
        };

        if (SetStateIfDifferent(newState))
            logger.LogDebug("Setting game state to {state} after period end", newState);

        if (newState.Stage == Stage.Intermission)
        {
            logger.LogInformation("Starting intermission with length {intermissionLength} seconds due to end of period", 15 * 60);
            await eventBus.AddEvent(Context.GameInfo, new IntermissionStarted(@event.Tick, new(15 * 60)));
        }
    }

    public void Handle(PeriodFinalized @event)
    {
        var state = GetState();
        var newState = state switch
        {
            (Stage.Intermission, _, _, false) => state with
            {
                JamNumber = 0,
                PeriodNumber = state.PeriodNumber + 1,
                PeriodIsFinalized = true,
            },
            (Stage.AfterGame, _, _, false) => state with {PeriodIsFinalized = true},
            _ => state
        };

        if (SetStateIfDifferent(newState))
            logger.LogDebug("Setting game state to {state} after period finalized", newState);
    }
}

public record GameStageState(Stage Stage, int PeriodNumber, int JamNumber, bool PeriodIsFinalized);

public enum Stage
{
    BeforeGame,
    Lineup,
    Jam,
    Timeout,
    Intermission,
    AfterGame,
}