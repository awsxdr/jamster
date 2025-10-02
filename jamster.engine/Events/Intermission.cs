using jamster.engine.Services;

namespace jamster.engine.Events;

public class IntermissionStarted(Guid7 id) : Event(id), IPeriodClockAligned;

public class IntermissionEnded(Guid7 id) : Event(id), IPeriodClockAligned;