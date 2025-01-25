export type ScoreSheetState = {
    jams: ScoreSheetJam[];
}

export type ScoreSheetJam = {
    period: number;
    jam: number;
    lineLabel: string;
    jammerNumber: string;
    currentTrip: number;
    lost: boolean;
    lead: boolean;
    called: boolean;
    injury: boolean;
    noInitial: boolean;
    trips: JamLineTrip[];
    jamTotal: string;
    gameTotal: string;
}

export type JamLineTrip = {
    score?: number;
}