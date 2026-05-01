import { useGameState } from "@/hooks";
import { Clock, ClockProps } from "./Clock";
import { LineupClockState } from "@/types";

type LineupClockProps = Omit<ClockProps, "seconds" | "isRunning" | "direction" | "startValue">;

export const LineupClock = (props: LineupClockProps) => {

    const clockState = useGameState<LineupClockState>("LineupClockState");

    return (
        <Clock
            id="LineupClock"
            seconds={clockState?.secondsPassed} 
            isRunning={clockState?.isRunning ?? false}
            direction="up" 
            {...props} 
        />
    );
}