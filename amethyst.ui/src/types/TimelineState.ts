import { Stage } from "./Stage"

export type TimelineState = {
    currentStage: Stage;
    currentStageStartTick: number;
    currentStageEventId: string;
    previousStages: StageListItem[];
}

export type StageListItem = {
    stage: Stage;
    startTick: number;
    duration: number;
    eventId: string;
}