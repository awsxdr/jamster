using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Extensions;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public abstract class PreviousJamLineup(TeamSide teamSide, ReducerGameContext context)
    : Reducer<PreviousJamLineupState>(context)
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<PreviousJamSkaterOnTrack>
    , IDependsOnState<JamLineupState>
{
    protected override PreviousJamLineupState DefaultState => new(new(null, null, [null, null, null]));
    public override Option<string> GetStateKey() => Option.Some(teamSide.ToString());

    public IEnumerable<Event> Handle(JamEnded @event)
    {
        var previousLineup = GetKeyedState<JamLineupState>(teamSide.ToString());

        SetState(new(previousLineup));

        return [];
    }

    public IEnumerable<Event> Handle(PreviousJamSkaterOnTrack @event) => @event.HandleIfTeam(teamSide, () =>
    {
        var state = GetState();

        var position =
            state.Lineup.JammerNumber == @event.Body.SkaterNumber ? SkaterPosition.Jammer
            : state.Lineup.PivotNumber == @event.Body.SkaterNumber ? SkaterPosition.Pivot
            : SkaterPosition.Blocker;

        return [new SkaterOnTrack(@event.Tick, new(teamSide, @event.Body.SkaterNumber, position))];
    });
}

public sealed record PreviousJamLineupState(JamLineupState Lineup);

public class HomeTeamPreviousJamLineup(ReducerGameContext context) : PreviousJamLineup(TeamSide.Home, context);
public class AwayTeamPreviousJamLineup(ReducerGameContext context) : PreviousJamLineup(TeamSide.Away, context);
