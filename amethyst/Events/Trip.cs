using amethyst.Domain;
using amethyst.Services;

namespace amethyst.Events;

public sealed class LastTripDeleted(Guid7 id, LastTripDeletedBody body) : Event<LastTripDeletedBody>(id, body);
public sealed record LastTripDeletedBody(TeamSide Side) : TeamEventBody(Side);
public sealed class TripCompleted(Guid7 id, TripCompletedBody body) : Event<TripCompletedBody>(id, body);
public sealed record TripCompletedBody(TeamSide Side) : TeamEventBody(Side);