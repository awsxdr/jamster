import { Stage } from ".";

export type GameStageState = {
    stage: Stage,
    periodNumber: number,
    jamNumber: number,
    totalJamNumber: number,
    isInOvertime: boolean,
    periodIsFinalized: boolean,
};
