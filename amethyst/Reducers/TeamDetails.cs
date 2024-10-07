using amethyst.DataStores;
using amethyst.Events;
using amethyst.Services;

namespace amethyst.Reducers;

public abstract class TeamDetails(TeamSide teamSide, GameContext context, ILogger logger) 
    : Reducer<TeamDetailsState>(context)
    , IHandlesEvent<TeamSet>
{
    protected override TeamDetailsState DefaultState => new(new Team(Guid.NewGuid(), [], [], []));

    public void Handle(TeamSet @event) => HandleIfTeam(@event, () =>
    {
        SetState(new (@event.Body.Team));
    });

    private void HandleIfTeam<TEvent>(TEvent @event, Action handler) where TEvent : Event
    {
        if (@event.HasBody && @event.GetBodyObject() is TeamEventBody teamEventBody && teamEventBody.TeamSide != teamSide)
            return;

        handler();
    }
}

public sealed record TeamDetailsState(Team Team);

public sealed class HomeTeamDetails(GameContext context, ILogger<HomeTeamDetails> logger) : TeamDetails(TeamSide.Home, context, logger);
public sealed class AwayTeamDetails(GameContext context, ILogger<HomeTeamDetails> logger) : TeamDetails(TeamSide.Away, context, logger);
