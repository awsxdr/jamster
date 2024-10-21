using amethyst.DataStores;
using amethyst.Domain;
using amethyst.Events;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class TeamDetails(TeamSide teamSide, GameContext context, ILogger logger) 
    : Reducer<TeamDetailsState>(context)
    , IHandlesEvent<TeamSet>
{
    protected override TeamDetailsState DefaultState => new(new Team(
        Guid.NewGuid(),
        new() { ["default"] = teamSide == TeamSide.Home ? "Black" : "White" },
        [],
        []));

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(TeamSet @event) => HandleIfTeam(@event, () =>
    {
        SetState(new (@event.Body.Team));

        return [];
    });

    private IEnumerable<Event> HandleIfTeam<TEvent>(TEvent @event, Func<IEnumerable<Event>> handler) where TEvent : Event
    {
        if (@event.HasBody && @event.GetBodyObject() is TeamEventBody teamEventBody && teamEventBody.TeamSide != teamSide)
            return [];

        return handler();
    }
}

public sealed record TeamDetailsState(Team Team);

public sealed class HomeTeamDetails(GameContext context, ILogger<HomeTeamDetails> logger) : TeamDetails(TeamSide.Home, context, logger);
public sealed class AwayTeamDetails(GameContext context, ILogger<AwayTeamDetails> logger) : TeamDetails(TeamSide.Away, context, logger);
