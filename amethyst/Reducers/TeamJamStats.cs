using amethyst.Domain;
using amethyst.Events;
using amethyst.Extensions;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class TeamJamStats(TeamSide teamSide, ReducerGameContext gameContext, ILogger logger)
    : Reducer<TeamJamStatsState>(gameContext)
    , IHandlesEvent<LeadMarked>
    , IHandlesEvent<LostMarked>
    , IHandlesEvent<CallMarked>
    , IHandlesEvent<StarPassMarked>
    , IHandlesEvent<InitialTripCompleted>
    , IHandlesEvent<JamStarted>
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
                (TeamSide.Home, _) when teamSide == TeamSide.Home => @event.Body.Lead,
                (TeamSide.Away, _) when teamSide == TeamSide.Away => @event.Body.Lead,
                (TeamSide.Home, true) when teamSide == TeamSide.Away => false,
                (TeamSide.Away, true) when teamSide == TeamSide.Home => false,
                _ => state.Lead
            };

        SetState(state with { Lead = lead });

        return [];
    }

    public IEnumerable<Event> Handle(LostMarked @event) => @event.HandleIfTeam(teamSide, () =>
    {
        SetState(GetState() with { Lost = @event.Body.Lost });

        return [];
    });

    public IEnumerable<Event> Handle(CallMarked @event) => @event.HandleIfTeam(teamSide, () =>
    {
        SetState(GetState() with { Called = @event.Body.Call });

        return [];
    });

    public IEnumerable<Event> Handle(StarPassMarked @event) => @event.HandleIfTeam(teamSide, () =>
    {
        SetState(GetState() with { StarPass = @event.Body.StarPass });

        return [];
    });

    public IEnumerable<Event> Handle(InitialTripCompleted @event) => @event.HandleIfTeam(teamSide, () =>
    {
        SetState(GetState() with { HasCompletedInitial = @event.Body.TripCompleted });

        return [];
    });

    public IEnumerable<Event> Handle(JamStarted @event)
    {
        SetState((TeamJamStatsState) GetDefaultState());

        return [];
    }
}

public record TeamJamStatsState(bool Lead, bool Lost, bool Called, bool StarPass, bool HasCompletedInitial);

public sealed class HomeTeamJamStats(ReducerGameContext gameContext, ILogger<HomeTeamJamStats> logger) : TeamJamStats(TeamSide.Home, gameContext, logger);
public sealed class AwayTeamJamStats(ReducerGameContext gameContext, ILogger<AwayTeamJamStats> logger) : TeamJamStats(TeamSide.Away, gameContext, logger);