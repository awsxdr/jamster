using amethyst.Services;

namespace amethyst.Events;

public sealed class JamStarted(Guid7 id) : Event(id), IPeriodClockAligned, IShownInUndo;
public sealed class JamEnded(Guid7 id) : Event(id), IPeriodClockAligned, IShownInUndo;