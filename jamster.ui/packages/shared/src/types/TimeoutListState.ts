import { TeamSide } from "./TeamSide";
import { TimeoutType } from "./TimeoutType";

export type TimeoutListState = {
    timeouts: TimeoutListItem[];
};

export type TimeoutListItem = {
    eventId: string;
    type: TimeoutType;
    period: number;
    jam: number;
    side?: TeamSide;
    durationInSeconds?: number;
    retained: boolean;
}