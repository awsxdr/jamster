export type PeriodClockState = {
    isRunning: boolean,
    hasExpired: boolean,
    startTick: number,
    ticksPassed: number,
    secondsPassed: number,
};

