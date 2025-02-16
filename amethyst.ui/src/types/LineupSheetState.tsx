export type LineupSheetState = {
    jams: LineupSheetJam[];
}

export type LineupSheetJam = {
    period: number;
    jam: number;
    jammerNumber: string | null;
    pivotNumber: string | null;
    blockerNumbers: (string | null)[];
}