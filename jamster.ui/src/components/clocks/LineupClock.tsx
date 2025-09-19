import { useLineupClockState } from "@/hooks";
import { Clock, ClockProps } from "./Clock";

type LineupClockState = {
    isRunning: boolean,
    startTick: number,
    ticksPassed: number,
    secondsPassed: number,
};

type LineupClockProps = Omit<ClockProps<LineupClockState>, "secondsMapper" | "stateName" | "direction" | "startValue">;

export const LineupClock = (props: LineupClockProps) => {

    const clockState = useLineupClockState();

    return (
        <Clock
            secondsMapper={s => s.secondsPassed} 
            state={clockState}
            direction="up" 
            {...props} 
        />
    );
}