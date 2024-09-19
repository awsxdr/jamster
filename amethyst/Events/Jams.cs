namespace amethyst.Events;

public class JamStarted(long tick) : Event(tick); 
public class JamEnded(long tick) : Event(tick);