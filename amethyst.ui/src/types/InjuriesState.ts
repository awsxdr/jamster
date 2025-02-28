export type InjuriesState = {
    injuries: Injury[];
}

type Injury = {
    skaterNumber: string;
    period: number;
    jam: number;
    totalJamNumberStart: number;
    expired: boolean;
}