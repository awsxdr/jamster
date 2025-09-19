using jamster.Domain;
using jamster.Services;

namespace jamster.Events;

public sealed class LeadMarked(Guid7 id, LeadMarkedBody body) : Event<LeadMarkedBody>(id, body);
public sealed record LeadMarkedBody(TeamSide TeamSide, bool Lead) : TeamEventBody(TeamSide);

public sealed class LostMarked(Guid7 id, LostMarkedBody body) : Event<LostMarkedBody>(id, body);
public sealed record LostMarkedBody(TeamSide TeamSide, bool Lost) : TeamEventBody(TeamSide);

public sealed class CallMarked(Guid7 id, CallMarkedBody body) : Event<CallMarkedBody>(id, body);
public sealed record CallMarkedBody(TeamSide TeamSide, bool Call) : TeamEventBody(TeamSide);

public sealed class StarPassMarked(Guid7 id, StarPassMarkedBody body) : Event<StarPassMarkedBody>(id, body);
public sealed record StarPassMarkedBody(TeamSide TeamSide, bool StarPass) : TeamEventBody(TeamSide);

public sealed class InitialTripCompleted(Guid7 id, InitialTripCompletedBody body) : Event<InitialTripCompletedBody>(id, body);
public sealed record InitialTripCompletedBody(TeamSide TeamSide, bool TripCompleted) : TeamEventBody(TeamSide);