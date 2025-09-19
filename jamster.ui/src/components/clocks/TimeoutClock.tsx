import { useTimeoutClockState } from "@/hooks";
import { Clock, ClockProps } from "./Clock";

type TimeoutClockState = {
    isRunning: boolean,
    startTick: number,
    ticksPassed: number,
    secondsPassed: number,
};

type TimeoutClockProps = Omit<ClockProps<TimeoutClockState>, "secondsMapper" | "stateName" | "direction" | "startValue">;

export const TimeoutClock = (props: TimeoutClockProps) => {

    const clockState = useTimeoutClockState();

    return (
        <Clock
            secondsMapper={s => s.secondsPassed} 
            state={clockState}
            direction="up" 
            {...props} 
        />
    );
}