using amethyst.Services;

namespace amethyst.Events;

/// <summary>
/// The period clock has expired
/// </summary>
public sealed class PeriodEnded(Guid7 id) : Event(id), IPeriodClockAligned;

/// <summary>
/// The period is finalized and the game should move onto the next period
/// </summary>
public sealed class PeriodFinalized(Guid7 id) : Event(id), IShownInUndo;