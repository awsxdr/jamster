﻿using jamster.engine.Domain;
using jamster.engine.Services;

namespace jamster.engine.Events;

public sealed class LastTripDeleted(Guid7 id, LastTripDeletedBody body) : Event<LastTripDeletedBody>(id, body);
public sealed record LastTripDeletedBody(TeamSide TeamSide) : TeamEventBody(TeamSide);
public sealed class TripCompleted(Guid7 id, TripCompletedBody body) : Event<TripCompletedBody>(id, body);
public sealed record TripCompletedBody(TeamSide TeamSide) : TeamEventBody(TeamSide);