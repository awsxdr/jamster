using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Extensions;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public abstract class TeamJamStats(TeamSide teamSide, ReducerGameContext gameContext, ILogger logger)
    : Reducer<TeamJamStatsState>(gameContext)
    , IHandlesEvent<LeadMarked>
    , IHandlesEvent<LostMarked>
    , IHandlesEvent<CallMarked>
    , IHandlesEvent<StarPassMarked>
    , IHandlesEvent<InitialTripCompleted>
    , IHandlesEvent<JamStarted>
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<ScoreModifiedRelative>
    , IDependsOnState<TimeoutClockState>
    , IDependsOnState<JamClockState>
{
    protected override TeamJamStatsState DefaultState => new(false, false, false, false, false);

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(LeadMarked @event)
    {
        var state = GetState();

        var lead =
            @event.Body switch
            {
                _ when teamSide == @event.Body.TeamSide => @event.Body.Lead,
                (_, true) when teamSide != @event.Body.TeamSide => false,
                _ => state.Lead
            };

        SetState(state with { Lead = lead });

        if (state.HasCompletedInitial || !lead) return [];

        logger.LogDebug("Initial trip completed set to {value} for {teamSide} due to lead marked", lead, teamSide);

        return [new InitialTripCompleted(@event.Tick, new(teamSide, true))];
    }

    public IEnumerable<Event> Handle(LostMarked @event) => @event.HandleIfTeam(teamSide, () =>
    {
        SetState(GetState() with { Lost = @event.Body.Lost });

        return [];
    });

    public IEnumerable<Event> Handle(CallMarked @event)
    {
        var state = GetState();

        var call =
            @event.Body switch
            {
                _ when teamSide == @event.Body.TeamSide => @event.Body.Call,
                (_, true) when teamSide != @event.Body.TeamSide => false,
                _ => state.Called
            };

        logger.LogDebug("Call set to {value} for {teamSide}", call, teamSide);

        SetState(state with { Called = call });

        return [];
    }

    public IEnumerable<Event> Handle(StarPassMarked @event) => @event.HandleIfTeam(teamSide, () =>
    {
        SetState(GetState() with { StarPass = @event.Body.StarPass });

        return [];
    });

    public IEnumerable<Event> Handle(InitialTripCompleted @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();
        var opponentState = GetKeyedState<TeamJamStatsState>(teamSide == TeamSide.Home ? nameof(TeamSide.Away) : nameof(TeamSide.Home));
        SetState(state with { HasCompletedInitial = @event.Body.TripCompleted, Lost = state.Lost || !state.Lead && !opponentState.Lead});

        return [];
    });

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        SetState((TeamJamStatsState) GetDefaultState());

        return [];
    }

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        var state = GetState();
        var timeoutClock = GetState<TimeoutClockState>();
        var jamClock = GetState<JamClockState>();

        if (state is { Lead: true, Lost: false, Called: false } && timeoutClock is { IsRunning: false } && jamClock is { Expired: false })
            return [new CallMarked(@event.Tick, new(teamSide, true))];

        return [];
    }

    public IEnumerable<Event> Handle(ScoreModifiedRelative @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        if (state.HasCompletedInitial) return [];

        logger.LogDebug("Marking initial trip completed for {teamSide} due to score modified", teamSide);

        return [new InitialTripCompleted(@event.Tick, new(teamSide, true))];
    });
}

public record TeamJamStatsState(bool Lead, bool Lost, bool Called, bool StarPass, bool HasCompletedInitial);

public sealed class HomeTeamJamStats(ReducerGameContext gameContext, ILogger<HomeTeamJamStats> logger) : TeamJamStats(TeamSide.Home, gameContext, logger);
public sealed class AwayTeamJamStats(ReducerGameContext gameContext, ILogger<AwayTeamJamStats> logger) : TeamJamStats(TeamSide.Away, gameContext, logger);