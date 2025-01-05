export type IntermissionClockState = {
    isRunning: boolean;
    hasExpired: boolean;
    targetTick: number;
    secondsRemaining: number;
};
