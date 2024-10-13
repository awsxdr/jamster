import { Clock, ClockProps } from "./Clock";

type LineupClockState = {
    isRunning: boolean,
    startTick: number,
    ticksPassed: number,
    secondsPassed: number,
};

type LineupClockProps = Omit<ClockProps<LineupClockState>, "secondsMapper" | "stateName" | "direction" | "startValue">;

export const LineupClock = (props: LineupClockProps) => (
    <Clock<LineupClockState> secondsMapper={s => s.secondsPassed} stateName="LineupClockState" direction="up" {...props} />
);
