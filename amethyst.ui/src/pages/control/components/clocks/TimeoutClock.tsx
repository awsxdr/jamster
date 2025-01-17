import { TimeoutClockState } from "@/types";
import { Clock, ClockProps } from "./Clock";
import { useCurrentGame, useEvents, useGameState } from "@/hooks";
import { TimeoutClockSet } from "@/types/events";
import { useMemo } from "react";

type TimeoutClockProps = Omit<ClockProps, "seconds" | "isRunning" | "direction" | "startValue">;

export const TimeoutClock = (props: TimeoutClockProps) => {

    const clockState = useGameState<TimeoutClockState>("TimeoutClockState");
    const { currentGame } = useCurrentGame();

    const { sendEvent } = useEvents();

    const isRunning = useMemo(() => clockState?.isRunning ?? false, [clockState, currentGame]);

    const handleClockSet = (value: number) => {
        if(!currentGame) {
            return;
        }

        sendEvent(currentGame.id, new TimeoutClockSet(value));
    }

    return (
        <Clock
            seconds={clockState?.secondsPassed}
            isRunning={isRunning}
            direction="up" 
            onClockSet={handleClockSet}
            {...props} 
        />
    );
}