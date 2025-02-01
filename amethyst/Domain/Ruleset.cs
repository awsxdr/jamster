namespace amethyst.Domain;

public record Ruleset(
    PeriodRules PeriodRules,
    JamRules JamRules,
    LineupRules LineupRules,
    TimeoutRules TimeoutRules,
    PenaltyRules PenaltyRules,
    IntermissionRules IntermissionRules
);

public record PeriodRules(
    int PeriodCount,
    Tick Duration,
    PeriodEndBehavior PeriodEndBehavior
);

public record JamRules(
    bool ResetJamNumbersBetweenPeriods,
    Tick Duration
);

public record LineupRules(
    Tick Duration,
    Tick OvertimeDuration
);

public record TimeoutRules(
    Tick TeamTimeoutDuration,
    TimeoutPeriodClockStopBehavior PeriodClockBehavior,
    int TeamTimeoutAllowance,
    TimeoutResetBehavior ResetBehavior
);

public record PenaltyRules(
    int FoulOutPenaltyCount
);

public record IntermissionRules(
    Tick Duration
);

public enum PeriodEndBehavior
{
    Immediately,
    AnytimeOutsideJam,
    OnJamEnd,
    Manual,
}

[Flags]
public enum TimeoutPeriodClockStopBehavior
{
    All = OfficialTimeout | TeamTimeout | OfficialReview,
    OfficialTimeout = 1 << 0,
    TeamTimeout = 1 << 1,
    OfficialReview = 1 << 2,
}

public enum TimeoutResetBehavior
{
    Never,
    Period,
}