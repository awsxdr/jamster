using jamster.engine.Services;

namespace jamster.engine.Events;

public sealed class CurrentGameSet(Guid7 id, CurrentGameSetBody body) : Event<CurrentGameSetBody>(id, body);
public sealed record CurrentGameSetBody(Guid GameId);