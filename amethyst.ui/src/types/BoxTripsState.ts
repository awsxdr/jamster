import { SkaterPosition } from "./events";

export type BoxTripsState = {
    boxTrips: BoxTrip[];
}

export type BoxTrip = {
    period: number;
    jam: number;
    totalJamStart: number;
    skaterNumber: string;
    skaterPosition: SkaterPosition;
    durationInJams: number | null;
    substitutions: Substitution[];
    lastStartTick: number;
    ticksPassedAtLastStart: number;
    ticksPassed: number;
    secondsPassed: number;
}

type Substitution = {
    newNumber: string;
    totalJamNumber: number;
}