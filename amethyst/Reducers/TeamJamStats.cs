using amethyst.Domain;
using amethyst.Events;
using amethyst.Extensions;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class TeamJamStats(TeamSide teamSide, ReducerGameContext gameContext)
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
                _ when teamSide == @event.Body.Side => @event.Body.Lead,
                (_, true) when teamSide != @event.Body.Side => false,
                _ => state.Lead
            };

        SetState(state with { Lead = lead });

        if (state.HasCompletedInitial || !lead) return [];

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
                _ when teamSide == @event.Body.Side => @event.Body.Call,
                (_, true) when teamSide != @event.Body.Side => false,
                _ => state.Called
            };

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
        SetState(GetState() with { HasCompletedInitial = @event.Body.TripCompleted });

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

        if (state is { Lead: true, Called: false } && timeoutClock is { IsRunning: false })
            return [new CallMarked(@event.Tick, new(teamSide, true))];

        return [];
    }

    public IEnumerable<Event> Handle(ScoreModifiedRelative @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        if (state.HasCompletedInitial) return [];

        return [new InitialTripCompleted(@event.Tick, new(teamSide, true))];
    });
}

public record TeamJamStatsState(bool Lead, bool Lost, bool Called, bool StarPass, bool HasCompletedInitial);

public sealed class HomeTeamJamStats(ReducerGameContext gameContext) : TeamJamStats(TeamSide.Home, gameContext);
public sealed class AwayTeamJamStats(ReducerGameContext gameContext) : TeamJamStats(TeamSide.Away, gameContext);