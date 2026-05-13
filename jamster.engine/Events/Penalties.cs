using jamster.engine.Domain;
using jamster.engine.Services;

namespace jamster.engine.Events;

public sealed class SkaterSatInBox(Guid7 id, SkaterSatInBoxBody body) : Event<SkaterSatInBoxBody>(id, body);
public sealed record SkaterSatInBoxBody(TeamSide TeamSide, Guid SkaterId) : TeamEventBody(TeamSide);

public sealed class SkaterReleasedFromBox(Guid7 id, SkaterReleasedFromBoxBody body) : Event<SkaterReleasedFromBoxBody>(id, body);
public sealed record SkaterReleasedFromBoxBody(TeamSide TeamSide, Guid SkaterId) : TeamEventBody(TeamSide);

public sealed class SkaterSubstitutedInBox(Guid7 id, SkaterSubstitutedInBoxBody body) : Event<SkaterSubstitutedInBoxBody>(id, body);
public sealed record SkaterSubstitutedInBoxBody(TeamSide TeamSide, Guid OriginalSkaterId, Guid NewSkaterId) : TeamEventBody(TeamSide);

public sealed class PenaltyAssessed(Guid7 id, PenaltyAssessedBody body) : Event<PenaltyAssessedBody>(id, body);
public sealed record PenaltyAssessedBody(TeamSide TeamSide, Guid SkaterId, string PenaltyCode) : TeamEventBody(TeamSide);

public sealed class PenaltyRescinded(Guid7 id, PenaltyRescindedBody body) : Event<PenaltyRescindedBody>(id, body);
public sealed record PenaltyRescindedBody(TeamSide TeamSide, Guid SkaterId, string PenaltyCode, int Period, int Jam) : TeamEventBody(TeamSide);

public sealed class PenaltyUpdated(Guid7 id, PenaltyUpdatedBody body) : Event<PenaltyUpdatedBody>(id, body);
public sealed record PenaltyUpdatedBody(TeamSide TeamSide, Guid SkaterId, string OriginalPenaltyCode, int OriginalPeriod, int OriginalJam, string NewPenaltyCode, int NewPeriod, int NewJam) : TeamEventBody(TeamSide);

public sealed class SkaterExpelled(Guid7 id, SkaterExpelledBody body) : Event<SkaterExpelledBody>(id, body);
public sealed record SkaterExpelledBody(TeamSide TeamSide, Guid SkaterId, string PenaltyCode, int Period, int Jam) : TeamEventBody(TeamSide);

public sealed class ExpulsionCleared(Guid7 id, ExpulsionClearedBody body) : Event<ExpulsionClearedBody>(id, body);
public sealed record ExpulsionClearedBody(TeamSide TeamSide, Guid SkaterId) : TeamEventBody(TeamSide);

public sealed class PenaltyServedSet(Guid7 id, PenaltyServedSetBody body) : Event<PenaltyServedSetBody>(id, body);
public sealed record PenaltyServedSetBody(TeamSide TeamSide, Guid SkaterId, string PenaltyCode, int Period, int Jam, bool Served) : TeamEventBody(TeamSide);