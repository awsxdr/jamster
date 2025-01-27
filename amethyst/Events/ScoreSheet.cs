using amethyst.Services;

namespace amethyst.Events;

public sealed class ScoreSheetTripScoreSet(Guid7 id, ScoreSheetTripScoreSetBody body) : Event<ScoreSheetTripScoreSetBody>(id, body);
public sealed record ScoreSheetTripScoreSetBody(int LineNumber, int TripNumber, string Value);

public sealed class ScoreSheetLeadSet(Guid7 id, ScoreSheetStatsSetBody body) : Event<ScoreSheetStatsSetBody>(id, body);
public sealed class ScoreSheetLostSet(Guid7 id, ScoreSheetStatsSetBody body) : Event<ScoreSheetStatsSetBody>(id, body);
public sealed class ScoreSheetCalledSet(Guid7 id, ScoreSheetStatsSetBody body) : Event<ScoreSheetStatsSetBody>(id, body);
public sealed class ScoreSheetInjurySet(Guid7 id, ScoreSheetStatsSetBody body) : Event<ScoreSheetStatsSetBody>(id, body);
public sealed class ScoreSheetNoInitialSet(Guid7 id, ScoreSheetStatsSetBody body) : Event<ScoreSheetStatsSetBody>(id, body);
public sealed record ScoreSheetStatsSetBody(int LineNumber, bool Value);

public sealed class ScoreSheetJammerNumberSet(Guid7 id, ScoreSheetJammerNumberSetBody body) : Event<ScoreSheetJammerNumberSetBody>(id, body);
public sealed record ScoreSheetJammerNumberSetBody(int LineNumber, string Value);