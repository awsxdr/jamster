using jamster.engine.Services;

namespace jamster.engine.Events;

/// <summary>
/// The period clock has expired
/// </summary>
public sealed class PeriodEnded(Guid7 id) : Event(id), IPeriodClockAligned;

/// <summary>
/// The period is finalized and the game should move onto the next period
/// </summary>
public sealed class PeriodFinalized(Guid7 id) : Event(id), IShownInUndo;

/// <summary>
/// Overtime has started and the period should be prevented from ending
/// </summary>
public sealed class OvertimeStarted(Guid7 id) : Event(id), IShownInUndo, IPeriodClockAligned;

/// <summary>
/// Overtime has ended
/// </summary>
public sealed class OvertimeEnded(Guid7 id) : Event(id), IShownInUndo, IPeriodClockAligned;