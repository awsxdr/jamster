using amethyst.Domain;
using amethyst.Services;

namespace amethyst.Events;

public sealed class LastTripDeleted(Guid7 id, LastTripDeletedBody body) : Event<LastTripDeletedBody>(id, body);
public sealed record LastTripDeletedBody(TeamSide Side) : TeamEventBody(Side);