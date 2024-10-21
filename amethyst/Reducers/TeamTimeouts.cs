using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class TeamTimeouts(TeamSide teamSide, GameContext context, ILogger logger) 
    : Reducer<TeamTimeoutsState>(context)
    , IHandlesEvent<PeriodFinalized>
    , IHandlesEvent<TimeoutTypeSet>
    , IHandlesEvent<TimeoutEnded>
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<JamStarted>
{
    protected override TeamTimeoutsState DefaultState => new(3, ReviewStatus.Unused, TimeoutInUse.None);

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(TimeoutTypeSet @event)
    {
        var state = GetState();

        var teamMatches = @event.Body.Side == teamSide;

        var newState = (@event.Body.Type, state.CurrentTimeout, teamMatches) switch
        {
            (TimeoutType.Team, TimeoutInUse.None, true) =>
                state with { CurrentTimeout = TimeoutInUse.Timeout, NumberRemaining = state.NumberRemaining - 1 },
            (TimeoutType.Review, TimeoutInUse.None, true) =>
                state with { CurrentTimeout = TimeoutInUse.Review },
            (TimeoutType.Review, TimeoutInUse.Timeout, true) => // Timeout changed to review for this team while in progress
                state with { CurrentTimeout = TimeoutInUse.Review, NumberRemaining = state.NumberRemaining + 1 },
            (TimeoutType.Team, TimeoutInUse.Timeout, false) =>  // Timeout changed to other team while in progress
                state with { CurrentTimeout = TimeoutInUse.None, NumberRemaining = state.NumberRemaining + 1 },
            (TimeoutType.Team, TimeoutInUse.Timeout, true) => // Timeout for this team repeated
                state,
            (TimeoutType.Review, TimeoutInUse.Review, true) => // Review for this team repeated
                state,
            (TimeoutType.Team, TimeoutInUse.Review, true) => // Review for this team changed to timeout
                state with { CurrentTimeout = TimeoutInUse.Timeout, NumberRemaining = state.NumberRemaining - 1 },
            (TimeoutType.Team, TimeoutInUse.Review, false) => // Review for this team changed to timeout
                state with { CurrentTimeout = TimeoutInUse.None },
            (_, TimeoutInUse.Timeout, _) => // Timeout changed to another type of timeout while in progress
                state with { CurrentTimeout = TimeoutInUse.None, NumberRemaining = state.NumberRemaining + 1 },
            (_, TimeoutInUse.Review, _) => // Review changed to another type of timeout while in progress
                state with { CurrentTimeout = TimeoutInUse.None },
            _ => state
        };

        logger.LogDebug("Setting timeout type to {type} for {teamSide} team", newState.CurrentTimeout, teamSide);

        SetStateIfDifferent(newState);

        return [];
    }

    public IEnumerable<Event> Handle(PeriodFinalized @event)
    {
        SetStateIfDifferent(GetState() with { ReviewStatus = ReviewStatus.Unused });

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutEnded @event)
    {
        EndTimeout();

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutStarted @event)
    {
        EndTimeout();

        return [];
    }

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        EndTimeout();

        return [];
    }

    private void EndTimeout()
    {
        var state = GetState();

        SetStateIfDifferent(state with
        {
            CurrentTimeout = TimeoutInUse.None,
            ReviewStatus = state.CurrentTimeout switch
            {
                TimeoutInUse.Review => ReviewStatus.Used,
                _ => state.ReviewStatus
            },
        });
    }
}

public sealed class HomeTeamTimeouts(GameContext context, ILogger<HomeTeamTimeouts> logger)
    : TeamTimeouts(TeamSide.Home, context, logger);

public sealed class AwayTeamTimeouts(GameContext context, ILogger<HomeTeamTimeouts> logger)
    : TeamTimeouts(TeamSide.Away, context, logger);

public sealed record TeamTimeoutsState(int NumberRemaining, ReviewStatus ReviewStatus, TimeoutInUse CurrentTimeout);

public enum TimeoutInUse
{
    None = 0,
    Timeout,
    Review
}