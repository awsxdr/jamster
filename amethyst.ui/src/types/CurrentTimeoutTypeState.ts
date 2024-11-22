import { TeamSide } from "./TeamSide"
import { TimeoutType } from "./TimeoutType"

export type CurrentTimeoutTypeState = {
    type: TimeoutType,
    side?: TeamSide,
}