export type PenaltySheetState = {
    lines: PenaltySheetLine[];
}

export type PenaltySheetLine = {
    skaterNumber: string;
    expulsionPenalty: Penalty | null,
    penalties: Penalty[];
}

export type Penalty = {
    code: string;
    period: number;
    jam: number;
    served: boolean;
}