import { useGameState } from "@/hooks";
import { Clock, ClockProps } from "./Clock";
import { IntermissionClockState } from "@/types/IntermissionClockState";

type IntermissionClockProps = Omit<ClockProps, "seconds" | "isRunning" | "direction" | "startValue">

export const IntermissionClock = (props: IntermissionClockProps) => {

    const clockState = useGameState<IntermissionClockState>("IntermissionClockState");

    return (
        <Clock 
            id="IntermissionClock"
            seconds={clockState?.secondsRemaining}
            isRunning={clockState?.isRunning ?? false}
            direction="up"
            {...props} 
        />
    );
}