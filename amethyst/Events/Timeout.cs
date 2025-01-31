using amethyst.Domain;
using amethyst.Services;

namespace amethyst.Events;

public sealed class TimeoutStarted(Guid7 id) : Event(id), IPeriodClockAligned, IShownInUndo;
public sealed class TimeoutEnded(Guid7 id) : Event(id), IShownInUndo;

public sealed class TimeoutTypeSet(Guid7 id, TimeoutTypeSetBody body) : Event<TimeoutTypeSetBody>(id, body);
public sealed record TimeoutTypeSetBody(TimeoutType Type, TeamSide? TeamSide);

public sealed class TeamReviewRetained(Guid7 id, TeamReviewRetainedBody body) : Event<TeamReviewRetainedBody>(id, body);
public sealed record TeamReviewRetainedBody(TeamSide TeamSide, Guid7 TimeoutEventId) : TeamEventBody(TeamSide);

public sealed class TeamReviewLost(Guid7 id, TeamReviewLostBody body) : Event<TeamReviewLostBody>(id, body);
public sealed record TeamReviewLostBody(TeamSide TeamSide, Guid7 TimeoutEventId) : TeamEventBody(TeamSide);

public enum TimeoutType
{
    Untyped = 0,
    Team,
    Review,
    Official,
}