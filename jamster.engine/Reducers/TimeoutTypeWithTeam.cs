using jamster.Domain;
using jamster.Events;
using jamster.Services;

namespace jamster.Reducers;

public class TimeoutTypeWithTeam(ReducerGameContext context) 
    : Reducer<TimeoutTypeState>(context)
    , IHandlesEvent<TimeoutStarted>
    , IHandlesEvent<TimeoutTypeSet>
{
    protected override TimeoutTypeState DefaultState => new(CompoundTimeoutType.Untyped, 0);

    public IEnumerable<Event> Handle(TimeoutStarted @event)
    {
        SetState(new(CompoundTimeoutType.Untyped, @event.Tick));

        return [];
    }

    public IEnumerable<Event> Handle(TimeoutTypeSet @event)
    {
        SetState(GetState() with
        {
            TimeoutType = @event.Body switch
            {
                { Type: TimeoutType.Team, TeamSide: TeamSide.Home } => CompoundTimeoutType.HomeTeamTimeout,
                { Type: TimeoutType.Team, TeamSide: TeamSide.Away } => CompoundTimeoutType.AwayTeamTimeout,
                { Type: TimeoutType.Review, TeamSide: TeamSide.Home } => CompoundTimeoutType.HomeOfficialReview,
                { Type: TimeoutType.Review, TeamSide: TeamSide.Away } => CompoundTimeoutType.AwayOfficialReview,
                { Type: TimeoutType.Official } => CompoundTimeoutType.OfficialTimeout,
                _ => CompoundTimeoutType.Untyped
            }
        });

        return [];
    }
}

public record TimeoutTypeState(CompoundTimeoutType TimeoutType, Tick StartTick);

public enum CompoundTimeoutType
{
    HomeTeamTimeout,
    HomeOfficialReview,
    AwayTeamTimeout,
    AwayOfficialReview,
    OfficialTimeout,
    Untyped,
}