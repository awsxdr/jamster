using amethyst.Services;

namespace amethyst.Events;

public sealed class JamStarted(Guid7 id) : Event(id); 
public sealed class JamEnded(Guid7 id) : Event(id);