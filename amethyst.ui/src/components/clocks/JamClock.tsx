import { Clock, ClockProps } from "./Clock";

type JamClockState = {
    isRunning: boolean,
    startTick: number,
    ticksPassed: number,
    secondsPassed: number,
};

type JamClockProps = Omit<ClockProps<JamClockState>, "secondsMapper" | "stateName" | "direction" | "startValue">;

export const JamClock = (props: JamClockProps) => (
    <Clock<JamClockState> secondsMapper={s => s.secondsPassed} stateName="JamClockState" direction="down" startValue={120} {...props} />
);
