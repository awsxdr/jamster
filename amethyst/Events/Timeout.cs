using amethyst.Services;

namespace amethyst.Events;

public sealed class TimeoutStarted(Guid7 id) : Event(id), IPeriodClockAligned;
public sealed class TimeoutEnded(Guid7 id) : Event(id);
public sealed class TimeoutTypeSet(TimeoutType type, Guid7 id) : Event(id);

public enum TimeoutType
{
    Untyped = 0,
    HomeTeam,
    AwayTeam,
    HomeTeamReview,
    AwayTeamReview,
    Official,
}