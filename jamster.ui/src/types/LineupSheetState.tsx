export type LineupSheetState = {
    jams: LineupSheetJam[];
}

export type LineupSheetJam = {
    period: number;
    jam: number;
    jammerId: string | null;
    pivotId: string | null;
    blockerIds: (string | null)[];
}