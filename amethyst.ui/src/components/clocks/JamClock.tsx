import { Clock } from "./Clock";

type JamClockState = {
    isRunning: boolean,
    startTick: number,
    ticksPassed: number,
    secondsPassed: number,
};

export const JamClock = () => (
    <Clock<JamClockState> secondsMapper={s => s.secondsPassed} stateName="JamClockState" direction="down" startValue={120} />
);
