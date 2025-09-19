using jamster.Domain;
using jamster.Services;

namespace jamster.Events;

public sealed class SkaterOnTrack(Guid7 id, SkaterOnTrackBody body) : Event<SkaterOnTrackBody>(id, body);
public sealed record SkaterOnTrackBody(TeamSide TeamSide, string SkaterNumber, SkaterPosition Position) : TeamEventBody(TeamSide);

public sealed class SkaterOffTrack(Guid7 id, SkaterOffTrackBody body) : Event<SkaterOffTrackBody>(id, body);
public sealed record SkaterOffTrackBody(TeamSide TeamSide, string SkaterNumber) : TeamEventBody(TeamSide);

public sealed class SkaterAddedToJam(Guid7 id, SkaterAddedToJamBody body) : Event<SkaterAddedToJamBody>(id, body);
public sealed record SkaterAddedToJamBody(TeamSide TeamSide, int Period, int Jam, string SkaterNumber, SkaterPosition Position) : TeamEventBody(TeamSide);

public sealed class SkaterRemovedFromJam(Guid7 id, SkaterRemovedFromJamBody body) : Event<SkaterRemovedFromJamBody>(id, body);
public sealed record SkaterRemovedFromJamBody(TeamSide TeamSide, int Period, int Jam, string SkaterNumber) : TeamEventBody(TeamSide);

public sealed class SkaterInjuryAdded(Guid7 id, SkaterInjuryAddedBody body) : Event<SkaterInjuryAddedBody>(id, body);
public sealed record SkaterInjuryAddedBody(TeamSide TeamSide, string SkaterNumber) : TeamEventBody(TeamSide);

public sealed class SkaterInjuryRemoved(Guid7 id, SkaterInjuryRemovedBody body) : Event<SkaterInjuryRemovedBody>(id, body);
public sealed record SkaterInjuryRemovedBody(TeamSide TeamSide, string SkaterNumber, int TotalJamNumberStart) : TeamEventBody(TeamSide);

public sealed class PreviousJamSkaterOnTrack(Guid7 id, PreviousJamSkaterOnTrackBody body) : Event<PreviousJamSkaterOnTrackBody>(id, body);
public sealed record PreviousJamSkaterOnTrackBody(TeamSide TeamSide, string SkaterNumber) : TeamEventBody(TeamSide);