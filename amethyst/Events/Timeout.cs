using amethyst.Domain;
using amethyst.Services;

namespace amethyst.Events;

public sealed class TimeoutStarted(Guid7 id) : Event(id), IPeriodClockAligned;
public sealed class TimeoutEnded(Guid7 id) : Event(id);
public sealed class TimeoutTypeSet(Guid7 id, TimeoutTypeSetBody body) : Event<TimeoutTypeSetBody>(id, body);

public sealed record TimeoutTypeSetBody(TimeoutType Type, TeamSide? Side);

public enum TimeoutType
{
    Untyped = 0,
    Team,
    Review,
    Official,
}