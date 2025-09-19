import { usePeriodClockState } from "@/hooks";
import { Clock, ClockProps } from "./Clock";

type PeriodClockState = {
    isRunning: boolean,
    hasExpired: boolean,
    startTick: number,
    ticksPassed: number,
    secondsPassed: number,
};

type PeriodClockProps = Omit<ClockProps<PeriodClockState>, "secondsMapper" | "stateName" | "direction" | "startValue">;

export const PeriodClock = (props: PeriodClockProps) => {

    const clockState = usePeriodClockState();

    return (
        <Clock
            secondsMapper={s => s.secondsPassed} 
            state={clockState}
            direction="down" 
            startValue={30 * 60} 
            {...props} 
        />
    );
}