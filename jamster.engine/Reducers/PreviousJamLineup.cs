using jamster.engine.Domain;
using jamster.engine.Events;
using jamster.engine.Services;

namespace jamster.engine.Reducers;

public abstract class PreviousJamLineup(TeamSide teamSide, ReducerGameContext context)
    : Reducer<PreviousJamLineupState>(context)
    , IHandlesEvent<JamEnded>
    , IHandlesEvent<PreviousJamSkaterOnTrack>
    , IDependsOnState<JamLineupState>
    , IDependsOnState<TeamDetailsState>
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
            state.Lineup.JammerId == @event.Body.SkaterId ? SkaterPosition.Jammer
            : state.Lineup.PivotId == @event.Body.SkaterId ? SkaterPosition.Pivot
            : SkaterPosition.Blocker;

        return [new SkaterOnTrack(@event.Tick, new(teamSide, @event.Body.SkaterId, position))];
    });
}

public sealed record PreviousJamLineupState(JamLineupState Lineup);

public class HomeTeamPreviousJamLineup(ReducerGameContext context) : PreviousJamLineup(TeamSide.Home, context);
public class AwayTeamPreviousJamLineup(ReducerGameContext context) : PreviousJamLineup(TeamSide.Away, context);
