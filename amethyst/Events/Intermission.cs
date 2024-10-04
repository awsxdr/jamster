using amethyst.Services;

namespace amethyst.Events;

public class IntermissionStarted(Guid7 id, IntermissionStartedBody body) : Event<IntermissionStartedBody>(id, body);
public record IntermissionStartedBody(int DurationInSeconds);

public class IntermissionLengthSet(Guid7 id, IntermissionLengthSetBody body) : Event<IntermissionLengthSetBody>(id, body);
public record IntermissionLengthSetBody(int DurationInSeconds);

public class IntermissionEnded(Guid7 id) : Event(id);