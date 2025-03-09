using amethyst.Services;

namespace amethyst.Events;

public sealed class JamStarted(Guid7 id) : Event(id), IPeriodClockAligned, IShownInUndo;

public sealed class JamEnded(Guid7 id) : Event(id), IPeriodClockAligned, IShownInUndo;

public sealed class JamExpired(Guid7 id): Event(id), IReplaceOnDelete<JamAutoExpiryDisabled>, IAlwaysPersisted
{
    public JamAutoExpiryDisabled GetDeletionReplacement() =>
        new(Tick - 1);
}

public sealed class JamAutoExpiryDisabled(Guid7 id) : Event(id);