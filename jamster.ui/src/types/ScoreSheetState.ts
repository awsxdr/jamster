export type ScoreSheetState = {
    jams: ScoreSheetJam[];
}

export type ScoreSheetJam = {
    period: number;
    jam: number;
    jammerNumber: string;
    pivotNumber: string;
    lost: boolean;
    lead: boolean;
    called: boolean;
    injury: boolean;
    noInitial: boolean;
    trips: JamLineTrip[];
    starPassTrip: number | null;
    jamTotal: number;
    gameTotal: number;
    deleted: boolean;
}

export type JamLineTrip = {
    score?: number;
}