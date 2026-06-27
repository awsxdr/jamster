using jamster.engine.Domain;
using jamster.engine.Events;
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
    , IHandlesEvent<TripCompleted>
    , IDependsOnState<TimeoutClockState>
    , IDependsOnState<JamClockState>
    , IDependsOnState<TeamScoreState>
    , IDependsOnState<OvertimeState>
{
    protected override TeamJamStatsState DefaultState => new(false, false, false, false, false);

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(LeadMarked @event)
    {
        var state = GetState();

        var events = new List<Event>();

        if (@event.Body.TeamSide == teamSide)
        {
            SetState(state with { Lead = @event.Body.Lead });

            if (!state.HasCompletedInitial && @event.Body.Lead)
            {
                logger.LogDebug("Initial trip completed set to true for {teamSide} due to lead marked", teamSide);
                events.Add(new InitialTripCompleted(@event.Tick, new(teamSide, true)));
            }
        }
        else if (@event.Body.Lead && state.Lead)
        {
            events.Add(new LeadMarked(@event.Tick, new(teamSide, false)));
        }

        return events;
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
        SetState(state with
        {
            HasCompletedInitial = @event.Body.TripCompleted, 
            Lost = (state.Lost, state.Lead, opponentState.Lead, @event.Body.TripCompleted) switch
            {
                (true, _, _, true) => true,
                (_, false, false, true) => true,
                _ => false
            }
        });

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

        var overtime = GetState<OvertimeState>();
        if (overtime.IsInOvertime)
            return [];

        if (state is { Lead: true, Lost: false, Called: false } && timeoutClock is { IsRunning: false } && jamClock is { Expired: false })
            return [new CallMarked(@event.Tick, new(teamSide, true))];

        return [];
    }

    public IEnumerable<Event> Handle(ScoreModifiedRelative @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        if (state.HasCompletedInitial) return [];

        var overtime = GetState<OvertimeState>();

        if (overtime.IsInOvertime) return [];

        logger.LogDebug("Marking initial trip completed for {teamSide} due to score modified", teamSide);

        return [new InitialTripCompleted(@event.Tick, new(teamSide, true))];
    });

    public IEnumerable<Event> Handle(TripCompleted @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        if (state.HasCompletedInitial) return [];

        logger.LogDebug("Marking initial trip completed for {teamSide} due to trip completed", teamSide);

        return [new InitialTripCompleted(@event.Tick, new(teamSide, true))];
    });
}

public record TeamJamStatsState(bool Lead, bool Lost, bool Called, bool StarPass, bool HasCompletedInitial);

public sealed class HomeTeamJamStats(ReducerGameContext gameContext, ILogger<HomeTeamJamStats> logger) : TeamJamStats(TeamSide.Home, gameContext, logger);
public sealed class AwayTeamJamStats(ReducerGameContext gameContext, ILogger<AwayTeamJamStats> logger) : TeamJamStats(TeamSide.Away, gameContext, logger);