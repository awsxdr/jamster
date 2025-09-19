import { DisplaySide } from "./DisplaySide";

export type ControlPanelViewConfiguration = {
    showClockControls: boolean;
    showScoreControls: boolean;
    showStatsControls: boolean;
    showLineupControls: boolean;
    showClocks: boolean;
    showTimeoutList: boolean;
    showScoreSheet: boolean;
    showTimeline: boolean;
    displaySide: DisplaySide;
}

export const DEFAULT_CONTROL_PANEL_VIEW_CONFIGURATION: ControlPanelViewConfiguration = {
    showClockControls: true,
    showScoreControls: true,
    showStatsControls: true,
    showLineupControls: true,
    showClocks: true,
    showTimeoutList: true,
    showScoreSheet: true,
    showTimeline: false,
    displaySide: DisplaySide.Both,
}