using jamster.Services;

namespace jamster.Events;

public sealed class IntermissionClockSet(Guid7 id, IntermissionClockSetBody body) : Event<IntermissionClockSetBody>(id, body);
public sealed record IntermissionClockSetBody(int SecondsRemaining);


public sealed class JamClockSet(Guid7 id, JamClockSetBody body) : Event<JamClockSetBody>(id, body), IPeriodClockAligned;
public sealed record JamClockSetBody(int SecondsRemaining);

public sealed class LineupClockSet(Guid7 id, LineupClockSetBody body) : Event<LineupClockSetBody>(id, body), IPeriodClockAligned;
public sealed record LineupClockSetBody(int SecondsPassed);

public sealed class PeriodClockSet(Guid7 id, PeriodClockSetBody body) : Event<PeriodClockSetBody>(id, body), IPeriodClockAligned;
public sealed record PeriodClockSetBody(int SecondsRemaining);

public sealed class TimeoutClockSet(Guid7 id, TimeoutClockSetBody body) : Event<TimeoutClockSetBody>(id, body), IPeriodClockAligned;
public sealed record TimeoutClockSetBody(int SecondsPassed);

