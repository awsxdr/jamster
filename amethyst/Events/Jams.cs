namespace amethyst.Events;

public sealed class JamStarted(long tick) : Event(tick); 
public sealed class JamEnded(long tick) : Event(tick);