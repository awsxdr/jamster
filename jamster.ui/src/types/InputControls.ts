export type InputControls = {
    clocks: ClockControls;
    homeScore: ScoreControls;
    awayScore: ScoreControls;
    homeStats: StatsControls;
    awayStats: StatsControls;
}

export type InputControlsItem = ClockControls | ScoreControls | StatsControls;

type ClockControls = {
    start: Control;
    stop: Control;
    timeout: Control;
    undo: Control;
}

type ScoreControls = {
    decrementScore: Control;
    incrementScore: Control;
    setTripScoreUnknown: Control;
    setTripScore0: Control;
    setTripScore1: Control;
    setTripScore2: Control;
    setTripScore3: Control;
    setTripScore4: Control;
}

type StatsControls = {
    lead: Control;
    lost: Control;
    called: Control;
    starPass: Control;
    initialTrip: Control;
}

export type Control = {
    inputType: InputType;
    binding?: string;
} | undefined;

export enum InputType {
    Keyboard = "Keyboard",
}

const DEFAULT_CLOCK_CONTROLS: ClockControls = {
    start: undefined,
    stop: undefined,
    timeout: undefined,
    undo: undefined,
}

const DEFAULT_SCORE_CONTROLS: ScoreControls = {
    decrementScore: undefined,
    incrementScore: undefined,
    setTripScoreUnknown: undefined,
    setTripScore0: undefined,
    setTripScore1: undefined,
    setTripScore2: undefined,
    setTripScore3: undefined,
    setTripScore4: undefined,
}

const DEFAULT_STATS_CONTROLS: StatsControls = {
    lead: undefined,
    lost: undefined,
    called: undefined,
    starPass: undefined,
    initialTrip: undefined,
}

export const DEFAULT_INPUT_CONTROLS: InputControls = {
    clocks: DEFAULT_CLOCK_CONTROLS,
    homeScore: DEFAULT_SCORE_CONTROLS,
    awayScore: DEFAULT_SCORE_CONTROLS,
    homeStats: DEFAULT_STATS_CONTROLS,
    awayStats: DEFAULT_STATS_CONTROLS,
}
