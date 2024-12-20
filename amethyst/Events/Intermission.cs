using amethyst.Services;

namespace amethyst.Events;

public class IntermissionStarted(Guid7 id, IntermissionStartedBody body) 
    : Event<IntermissionStartedBody>(id, body)
    , IPeriodClockAligned;
public record IntermissionStartedBody(int DurationInSeconds);

public class IntermissionEnded(Guid7 id) : Event(id);