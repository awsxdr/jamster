export type InjuriesState = {
    injuries: Injury[];
}

export type Injury = {
    skaterId: string;
    period: number;
    jam: number;
    totalJamNumberStart: number;
    expired: boolean;
}