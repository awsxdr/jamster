using amethyst.Events;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class TeamScore(Team team, GameContext gameContext, IEventBus eventBus, ILogger<JamClock> logger)
    : Reducer<TeamScoreState>(gameContext)
    , IHandlesEvent<ScoreModifiedRelative>
    , IHandlesEvent<ScoreSet>
{
    protected override TeamScoreState DefaultState => new(0);

    public override Option<string> GetStateKey() =>
        Option.Some(team.ToString());

    public void Handle(ScoreModifiedRelative @event) => HandleIfTeam(@event, () =>
    {
        var state = GetState();

        state = state with {Score = Math.Max(0, state.Score + @event.Body.Value)};

        SetState(state);
    });

    public void Handle(ScoreSet @event) => HandleIfTeam(@event, () =>
    {
        var state = GetState();

        state = state with {Score = Math.Max(0, @event.Body.Value)};

        SetState(state);
    });

    private void HandleIfTeam<TEvent>(TEvent @event, Action handler) where TEvent : Event
    {
        if (@event.HasBody && @event.GetBodyObject() is TeamEventBody teamEventBody && teamEventBody.Team != team)
            return;

        handler();
    }
}

public record TeamScoreState(int Score);

public enum Team
{
    Home,
    Away,
}

public sealed class HomeTeamScore(GameContext gameContext, IEventBus eventBus, ILogger<JamClock> logger)
    : TeamScore(Team.Home, gameContext, eventBus, logger);
public sealed class AwayTeamScore(GameContext gameContext, IEventBus eventBus, ILogger<JamClock> logger) 
    : TeamScore(Team.Away, gameContext, eventBus, logger);