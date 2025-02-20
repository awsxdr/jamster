export type GameSummaryState = {
    gameProgress: GameProgress;
    homeScore: ScoreSummary;
    awayScore: ScoreSummary;
    homePenalties: PenaltySummary;
    awayPenalties: PenaltySummary;
    periodJamCounts: number[];
}

export enum GameProgress {
    Upcoming = "Upcoming",
    InProgress = "InProgress",
    Finished = "Finished",
}

export type ScoreSummary = {
    periodTotals: number[];
    grandTotal: number;
}

export type PenaltySummary = {
    periodTotals: number[];
    grandTotal: number;
}