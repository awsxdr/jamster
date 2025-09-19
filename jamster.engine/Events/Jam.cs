using jamster.Services;

namespace jamster.Events;

public sealed class JamStarted(Guid7 id) : Event(id), IPeriodClockAligned, IShownInUndo;

public sealed class JamEnded(Guid7 id) : Event(id), IPeriodClockAligned, IShownInUndo;

public sealed class JamExpired(Guid7 id): Event(id), IReplaceOnDelete<JamAutoExpiryDisabled>, IAlwaysPersisted, IShownInUndo
{
    public JamAutoExpiryDisabled GetDeletionReplacement() =>
        new(Tick - 1);
}

public sealed class JamAutoExpiryDisabled(Guid7 id) : Event(id);

public sealed class JamNumberOffset(Guid7 id, JamNumberOffsetBody body) : Event<JamNumberOffsetBody>(id, body);
public sealed record JamNumberOffsetBody(int Period, int Offset);