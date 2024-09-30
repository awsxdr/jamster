using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public class GameStage(GameContext context) : Reducer<GameStageState>(context), IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<TimeoutStarted>
{
    protected override GameStageState DefaultState => new(Stage.BeforeGame);

    public void Handle(JamStarted @event) =>
        SetState(new(Stage.Jam));

    public void Handle(JamEnded @event)
    {
        throw new NotImplementedException();
    }

    public void Handle(TimeoutStarted @event)
    {
        throw new NotImplementedException();
    }
}

public record GameStageState(Stage Stage);

public enum Stage
{
    BeforeGame,
    Lineup,
    Jam,
    Timeout,
    Intermission,
    AfterGame,
}