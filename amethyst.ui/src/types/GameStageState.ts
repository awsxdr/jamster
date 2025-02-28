import { Stage } from ".";

export type GameStageState = {
    stage: Stage,
    periodNumber: number,
    jamNumber: number,
    totalJamNumber: number,
    periodIsFinalized: boolean,
};
