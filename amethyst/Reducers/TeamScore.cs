using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class TeamScore(TeamSide teamSide, GameContext gameContext, ILogger logger)
    : Reducer<TeamScoreState>(gameContext)
    , IHandlesEvent<ScoreModifiedRelative>
    , IHandlesEvent<ScoreSet>
{
    protected override TeamScoreState DefaultState => new(0);

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(ScoreModifiedRelative @event) => HandleIfTeam(@event, () =>
    {
        logger.LogDebug("Changing {teamSide} score by {points} points", teamSide, @event.Body.Value);

        var state = GetState();

        state = state with {Score = Math.Max(0, state.Score + @event.Body.Value)};

        SetState(state);

        return [];
    });

    public IEnumerable<Event> Handle(ScoreSet @event) => HandleIfTeam(@event, () =>
    {
        logger.LogDebug("Setting {teamSide} score to {points} points", teamSide, @event.Body.Value);

        var state = GetState();

        state = state with {Score = Math.Max(0, @event.Body.Value)};

        SetState(state);

        return [];
    });

    private IEnumerable<Event> HandleIfTeam<TEvent>(TEvent @event, Func<IEnumerable<Event>> handler) where TEvent : Event
    {
        if (@event.HasBody && @event.GetBodyObject() is TeamEventBody teamEventBody && teamEventBody.TeamSide != teamSide)
            return [];

        return handler();
    }
}

public record TeamScoreState(int Score);

public sealed class HomeTeamScore(GameContext gameContext, ILogger<HomeTeamScore> logger)
    : TeamScore(TeamSide.Home, gameContext, logger);
public sealed class AwayTeamScore(GameContext gameContext, ILogger<AwayTeamScore> logger) 
    : TeamScore(TeamSide.Away, gameContext, logger);