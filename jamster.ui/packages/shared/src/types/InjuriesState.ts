export type InjuriesState = {
    injuries: Injury[];
}

export type Injury = {
    skaterNumber: string;
    period: number;
    jam: number;
    totalJamNumberStart: number;
    expired: boolean;
}