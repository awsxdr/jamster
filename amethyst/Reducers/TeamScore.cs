using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public abstract class TeamScore(TeamSide teamSide, ReducerGameContext gameContext, ILogger logger)
    : Reducer<TeamScoreState>(gameContext)
    , IHandlesEvent<ScoreModifiedRelative>
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<LastTripDeleted>
    , IDependsOnState<TripScoreState>
{
    protected override TeamScoreState DefaultState => new(0, 0);

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(ScoreModifiedRelative @event) => @event.HandleIfTeam(teamSide, () =>
    {
        logger.LogDebug("Changing {teamSide} score by {points} points", teamSide, @event.Body.Value);

        var state = GetState();

        state = new TeamScoreState(Score: Math.Max(0, state.Score + @event.Body.Value), JamScore: Math.Max(0, state.JamScore + @event.Body.Value));

        SetState(state);

        return [];
    });

    public IEnumerable<Event> Handle(JamStarted _)
    {
        logger.LogDebug("Settings jam score to 0 due to jam start");

        SetState(GetState() with { JamScore = 0 });

        return [];
    }

    public IEnumerable<Event> Handle(LastTripDeleted @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var tripScore = GetCachedKeyedState<TripScoreState>(teamSide.ToString());

        var state = GetState();

        var newScoreValue = Math.Max(0, state.Score - (tripScore.Score ?? 0));
        var newJamScoreValue = Math.Max(0, state.JamScore - (tripScore.Score ?? 0));

        SetState(new (newScoreValue, newJamScoreValue));

        return [];
    });
}

public record TeamScoreState(int Score, int JamScore);

public sealed class HomeTeamScore(ReducerGameContext gameContext, ILogger<HomeTeamScore> logger)
    : TeamScore(TeamSide.Home, gameContext, logger);
public sealed class AwayTeamScore(ReducerGameContext gameContext, ILogger<AwayTeamScore> logger) 
    : TeamScore(TeamSide.Away, gameContext, logger);