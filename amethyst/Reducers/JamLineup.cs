using amethyst.Domain;
using amethyst.Events;
using amethyst.Extensions;
using amethyst.Services;
using Func;

namespace amethyst.Reducers;

public abstract class JamLineup(TeamSide teamSide, ReducerGameContext context, ILogger logger)
    : Reducer<JamLineupState>(context)
    , IHandlesEvent<SkaterOnTrack>
    , IHandlesEvent<JamEnded>
{
    protected override JamLineupState DefaultState => new(null, null);

    public override Option<string> GetStateKey() =>
        Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(SkaterOnTrack @event) => @event.HandleIfTeam(teamSide, () =>
    {
        logger.LogDebug("Setting {position} for {team} team to {skaterId}", @event.Body.Position, teamSide, @event.Body.SkaterNumber);

        var state = GetState();

        SetState(@event.Body.Position switch
        {
            SkaterPosition.Jammer => new(@event.Body.SkaterNumber, state.PivotNumber == @event.Body.SkaterNumber ? null : state.PivotNumber),
            SkaterPosition.Pivot => new(state.JammerNumber == @event.Body.SkaterNumber ? null : state.JammerNumber, @event.Body.SkaterNumber),
            _ => state
        });

        return [];
    });

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        logger.LogDebug("Clearing jam lineup for {team} team due to jam end", teamSide);

        SetState(DefaultState);

        return [];
    }
}

public record JamLineupState(string? JammerNumber, string? PivotNumber);

public sealed class HomeTeamJamLineup(ReducerGameContext context, ILogger<HomeTeamJamLineup> logger) : JamLineup(TeamSide.Home, context, logger);
public sealed class AwayTeamJamLineup(ReducerGameContext context, ILogger<AwayTeamJamLineup> logger) : JamLineup(TeamSide.Away, context, logger);
