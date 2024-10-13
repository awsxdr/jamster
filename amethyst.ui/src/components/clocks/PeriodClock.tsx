import { Clock, ClockProps } from "./Clock";

type PeriodClockState = {
    isRunning: boolean,
    hasExpired: boolean,
    startTick: number,
    ticksPassed: number,
    secondsPassed: number,
};

type PeriodClockProps = Omit<ClockProps<PeriodClockState>, "secondsMapper" | "stateName" | "direction" | "startValue">;

export const PeriodClock = (props: PeriodClockProps) => (
    <Clock<PeriodClockState> secondsMapper={s => s.secondsPassed} stateName="PeriodClockState" direction="down" startValue={30 * 60} {...props} />
);
