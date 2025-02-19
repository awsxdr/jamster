using amethyst.Domain;
using amethyst.Services;

namespace amethyst.Events;

public sealed class SkaterSatInBox(Guid7 id, SkaterSatInBoxBody body) : Event<SkaterSatInBoxBody>(id, body);
public sealed record SkaterSatInBoxBody(TeamSide TeamSide, string SkaterNumber) : TeamEventBody(TeamSide);

public sealed class SkaterReleasedFromBox(Guid7 id, SkaterReleasedFromBoxBody body) : Event<SkaterReleasedFromBoxBody>(id, body);
public sealed record SkaterReleasedFromBoxBody(TeamSide TeamSide, string SkaterNumber) : TeamEventBody(TeamSide);

public sealed class SkaterSubstitutedInBox(Guid7 id, SkaterSubstitutedInBoxBody body) : Event<SkaterSubstitutedInBoxBody>(id, body);
public sealed record SkaterSubstitutedInBoxBody(TeamSide TeamSide, string OriginalSkaterNumber, string NewSkaterNumber) : TeamEventBody(TeamSide);
