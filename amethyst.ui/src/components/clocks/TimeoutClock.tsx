import { Clock, ClockProps } from "./Clock";

type TimeoutClockState = {
    isRunning: boolean,
    startTick: number,
    ticksPassed: number,
    secondsPassed: number,
};

type TimeoutClockProps = Omit<ClockProps<TimeoutClockState>, "secondsMapper" | "stateName" | "direction" | "startValue">;

export const TimeoutClock = (props: TimeoutClockProps) => (
    <Clock<TimeoutClockState> secondsMapper={s => s.secondsPassed} stateName="TimeoutClockState" direction="up" {...props} />
);
