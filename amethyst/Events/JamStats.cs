using amethyst.Domain;
using amethyst.Services;

namespace amethyst.Events;

public sealed class LeadMarked(Guid7 id, LeadMarkedBody body) : Event<LeadMarkedBody>(id, body);
public sealed record LeadMarkedBody(TeamSide Side, bool Lead) : TeamEventBody(Side);

public sealed class LostMarked(Guid7 id, LostMarkedBody body) : Event<LostMarkedBody>(id, body);
public sealed record LostMarkedBody(TeamSide Side, bool Lost) : TeamEventBody(Side);

public sealed class CallMarked(Guid7 id, CallMarkedBody body) : Event<CallMarkedBody>(id, body);
public sealed record CallMarkedBody(TeamSide Side, bool Call) : TeamEventBody(Side);

public sealed class StarPassMarked(Guid7 id, StarPassMarkedBody body) : Event<StarPassMarkedBody>(id, body);
public sealed record StarPassMarkedBody(TeamSide Side, bool StarPass) : TeamEventBody(Side);

public sealed class InitialTripCompleted(Guid7 id, InitialTripCompletedBody body) : Event<InitialTripCompletedBody>(id, body);
public sealed record InitialTripCompletedBody(TeamSide Side, bool TripCompleted) : TeamEventBody(Side);