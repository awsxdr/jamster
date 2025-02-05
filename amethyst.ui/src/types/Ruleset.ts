export type RulesState = {
    rules: Ruleset;
}

export type Ruleset = {
    periodRules: PeriodRules;
    jamRules: JamRules;
    lineupRules: LineupRules;
    timeoutRules: TimeoutRules;
    penaltyRules: PenaltyRules;
    intermissionRules: IntermissionRules;
}

export type PeriodRules = {
    periodCount: number;
    durationInSeconds: number;
    periodEndBehavior: PeriodEndBehavior;
}

export type JamRules = {
    resetJamNumbersBetweenPeriods: boolean;
    durationInSeconds: number;
}

export type LineupRules = {
    durationInSeconds: number;
    overtimeDurationInSeconds: number;
}

export type TimeoutRules = {
    teamTimeoutDurationInSeconds: number;
    periodClockBehavior: TimeoutPeriodClockStopBehavior;
    teamTimeoutAllowance: number;
    resetBehavior: TimeoutResetBehavior;
}

export type PenaltyRules = {
    foulOutPenaltyCount: number;
}

export type IntermissionRules = {
    durationInSeconds: number;
}

export enum PeriodEndBehavior {
    Immediately = "Immediately",
    AnytimeOutsideJam = "AnytimeOutsideJam",
    OnJamEnd = "OnJamEnd",
    Manual = "Manual",
}

export enum TimeoutPeriodClockStopBehavior {
    OfficialTimeout = 1 << 0,
    TeamTimeout = 1 << 1,
    OfficialReview = 1 << 2,
    All = OfficialTimeout | TeamTimeout | OfficialReview,
}

export enum TimeoutResetBehavior {
    Never = "Never",
    Period = "Period",
}