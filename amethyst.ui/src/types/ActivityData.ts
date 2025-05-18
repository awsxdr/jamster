import { ClientActivity } from "./ClientActivity";

export type ActivityData = {
    gameId: string | null;
    languageCode: string;
} & (
    UnknownActivity 
    | ScoreboardActivity 
    | StreamOverlayActivity 
    | PenaltyWhiteboardActivity 
    | ScoreboardOperatorActivity 
    | PenaltyLineupControlActivity
    | PenaltyControlActivity
    | LineupControlActivity
    | BoxTimingActivity
    | OtherActivity
)

export type UnknownActivity = {
    activity: ClientActivity.Unknown;
}

export type ScoreboardActivity = {
    activity: ClientActivity.Scoreboard;
    useSidebars: boolean;
    useNameBackgrounds: boolean;
}

export type StreamOverlayActivity = {
    activity: ClientActivity.StreamOverlay;
    scale: number;
    useBackground: boolean;
    backgroundColor: string;
}

export type PenaltyWhiteboardActivity = {
    activity: ClientActivity.PenaltyWhiteboard;
}

export type ScoreboardOperatorActivity = {
    activity: ClientActivity.ScoreboardOperator;
}

export type PenaltyLineupControlActivity = {
    activity: ClientActivity.PenaltyLineupControl;
}

export type PenaltyControlActivity = {
    activity: ClientActivity.PenaltyControl;
}

export type LineupControlActivity = {
    activity: ClientActivity.LineupControl;
}

export type BoxTimingActivity = {
    activity: ClientActivity.BoxTiming;
}

export type OtherActivity = {
    activity: ClientActivity.Other;
}

