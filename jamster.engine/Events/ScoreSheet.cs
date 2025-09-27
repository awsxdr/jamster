using jamster.engine.Domain;
using jamster.engine.Services;

namespace jamster.engine.Events;

public sealed class ScoreSheetTripScoreSet(Guid7 id, ScoreSheetTripScoreSetBody body) : Event<ScoreSheetTripScoreSetBody>(id, body);
public sealed record ScoreSheetTripScoreSetBody(TeamSide TeamSide, int TotalJamNumber, int TripNumber, int? Value) : TeamEventBody(TeamSide);

public sealed class ScoreSheetLeadSet(Guid7 id, ScoreSheetStatsSetBody body) : Event<ScoreSheetStatsSetBody>(id, body);
public sealed class ScoreSheetLostSet(Guid7 id, ScoreSheetStatsSetBody body) : Event<ScoreSheetStatsSetBody>(id, body);
public sealed class ScoreSheetCalledSet(Guid7 id, ScoreSheetStatsSetBody body) : Event<ScoreSheetStatsSetBody>(id, body);
public sealed class ScoreSheetNoInitialSet(Guid7 id, ScoreSheetStatsSetBody body) : Event<ScoreSheetStatsSetBody>(id, body);
public sealed record ScoreSheetStatsSetBody(TeamSide TeamSide, int TotalJamNumber, bool Value) : TeamEventBody(TeamSide);

public sealed class ScoreSheetInjurySet(Guid7 id, ScoreSheetInjurySetBody body) : Event<ScoreSheetInjurySetBody>(id, body);
public sealed record ScoreSheetInjurySetBody(int TotalJamNumber, bool Value);

public sealed class ScoreSheetJammerNumberSet(Guid7 id, ScoreSheetSkaterNumberSetBody body) : Event<ScoreSheetSkaterNumberSetBody>(id, body);
public sealed class ScoreSheetPivotNumberSet(Guid7 id, ScoreSheetSkaterNumberSetBody body) : Event<ScoreSheetSkaterNumberSetBody>(id, body);
public sealed record ScoreSheetSkaterNumberSetBody(TeamSide TeamSide, int TotalJamNumber, string Value) : TeamEventBody(TeamSide);

public sealed class ScoreSheetStarPassTripSet(Guid7 id, ScoreSheetStarPassTripSetBody body) : Event<ScoreSheetStarPassTripSetBody>(id, body);
public sealed record ScoreSheetStarPassTripSetBody(TeamSide TeamSide, int TotalJamNumber, int? StarPassTrip) : TeamEventBody(TeamSide);

public sealed class ScoreSheetLineDeleted(Guid7 id, ScoreSheetLineDeletedBody body) : Event<ScoreSheetLineDeletedBody>(id, body);
public sealed record ScoreSheetLineDeletedBody(int TotalJamNumber);