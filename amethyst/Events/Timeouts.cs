namespace amethyst.Events;

public sealed class TimeoutStarted(long tick) : Event(tick);