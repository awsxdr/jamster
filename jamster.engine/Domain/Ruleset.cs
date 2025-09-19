using System.Text.Json.Serialization;

namespace jamster.Domain;

public record Ruleset(
    PeriodRules PeriodRules,
    JamRules JamRules,
    LineupRules LineupRules,
    TimeoutRules TimeoutRules,
    PenaltyRules PenaltyRules,
    IntermissionRules IntermissionRules,
    InjuryRules InjuryRules
);

public record PeriodRules(
    int PeriodCount,
    int DurationInSeconds,
    PeriodEndBehavior PeriodEndBehavior
);

public record JamRules(
    bool ResetJamNumbersBetweenPeriods,
    int DurationInSeconds
);

public record LineupRules(
    int DurationInSeconds,
    int OvertimeDurationInSeconds
);

public record TimeoutRules(
    int TeamTimeoutDurationInSeconds,
    TimeoutPeriodClockStopBehavior PeriodClockBehavior,
    int TeamTimeoutAllowance,
    TimeoutResetBehavior ResetBehavior
);

public record PenaltyRules(
    int FoulOutPenaltyCount
);

public record IntermissionRules(
    int DurationInSeconds
);

public record InjuryRules(
    int JamsToSitOutFollowingInjury,
    int NumberOfAllowableInjuriesPerPeriod
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