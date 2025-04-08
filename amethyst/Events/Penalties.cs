using amethyst.Domain;
using amethyst.Services;

namespace amethyst.Events;

public sealed class SkaterSatInBox(Guid7 id, SkaterSatInBoxBody body) : Event<SkaterSatInBoxBody>(id, body);
public sealed record SkaterSatInBoxBody(TeamSide TeamSide, string SkaterNumber) : TeamEventBody(TeamSide);

public sealed class SkaterReleasedFromBox(Guid7 id, SkaterReleasedFromBoxBody body) : Event<SkaterReleasedFromBoxBody>(id, body);
public sealed record SkaterReleasedFromBoxBody(TeamSide TeamSide, string SkaterNumber) : TeamEventBody(TeamSide);

public sealed class SkaterSubstitutedInBox(Guid7 id, SkaterSubstitutedInBoxBody body) : Event<SkaterSubstitutedInBoxBody>(id, body);
public sealed record SkaterSubstitutedInBoxBody(TeamSide TeamSide, string OriginalSkaterNumber, string NewSkaterNumber) : TeamEventBody(TeamSide);

public sealed class PenaltyAssessed(Guid7 id, PenaltyAssessedBody body) : Event<PenaltyAssessedBody>(id, body);
public sealed record PenaltyAssessedBody(TeamSide TeamSide, string SkaterNumber, string PenaltyCode) : TeamEventBody(TeamSide);

public sealed class PenaltyRescinded(Guid7 id, PenaltyRescindedBody body) : Event<PenaltyRescindedBody>(id, body);
public sealed record PenaltyRescindedBody(TeamSide TeamSide, string SkaterNumber, string PenaltyCode, int Period, int Jam) : TeamEventBody(TeamSide);

public sealed class PenaltyUpdated(Guid7 id, PenaltyUpdatedBody body) : Event<PenaltyUpdatedBody>(id, body);
public sealed record PenaltyUpdatedBody(TeamSide TeamSide, string SkaterNumber, string OriginalPenaltyCode, int OriginalPeriod, int OriginalJam, string NewPenaltyCode, int NewPeriod, int NewJam) : TeamEventBody(TeamSide);

public sealed class SkaterExpelled(Guid7 id, SkaterExpelledBody body) : Event<SkaterExpelledBody>(id, body);
public sealed record SkaterExpelledBody(TeamSide TeamSide, string SkaterNumber, string PenaltyCode, int Period, int Jam) : TeamEventBody(TeamSide);

public sealed class ExpulsionCleared(Guid7 id, ExpulsionClearedBody body) : Event<ExpulsionClearedBody>(id, body);
public sealed record ExpulsionClearedBody(TeamSide TeamSide, string SkaterNumber) : TeamEventBody(TeamSide);

public sealed class PenaltyServedSet(Guid7 id, PenaltyServedSetBody body) : Event<PenaltyServedSetBody>(id, body);
public sealed record PenaltyServedSetBody(TeamSide TeamSide, string SkaterNumber, string PenaltyCode, int Period, int Jam, bool Served) : TeamEventBody(TeamSide);