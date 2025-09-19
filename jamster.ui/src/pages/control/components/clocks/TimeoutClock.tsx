import { TimeoutClockState } from "@/types";
import { Clock, ClockProps } from "./Clock";
import { useGameState } from "@/hooks";
import { useMemo } from "react";

type TimeoutClockProps = Omit<ClockProps, "seconds" | "isRunning" | "direction" | "startValue">;

export const TimeoutClock = (props: TimeoutClockProps) => {

    const clockState = useGameState<TimeoutClockState>("TimeoutClockState");

    const isRunning = useMemo(() => clockState?.isRunning ?? false, [clockState]);

    return (
        <Clock
            seconds={clockState?.secondsPassed}
            isRunning={isRunning}
            direction="up" 
            {...props} 
        />
    );
}