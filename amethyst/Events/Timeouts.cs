namespace amethyst.Events;

public sealed class TimeoutStarted(long tick) : Event(tick);
public sealed class TimeoutEnded(long tick) : Event(tick);
public sealed class TimeoutTypeSet(TimeoutType type, long tick) : Event(tick);

public enum TimeoutType
{
    Untyped = 0,
    HomeTeam,
    AwayTeam,
    HomeTeamReview,
    AwayTeamReview,
    Official,
}