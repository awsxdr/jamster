import { TimeoutClockState } from "@/types";
import { Clock, ClockProps } from "./Clock";
import { useCurrentGame, useEvents, useGameState } from "@/hooks";
import { TimeoutClockSet } from "@/types/events";

type TimeoutClockProps = Omit<ClockProps, "seconds" | "isRunning" | "direction" | "startValue">;

export const TimeoutClock = (props: TimeoutClockProps) => {

    const clockState = useGameState<TimeoutClockState>("TimeoutClockState");
    const { currentGame } = useCurrentGame();

    const { sendEvent } = useEvents();

    const handleClockSet = (value: number) => {
        if(!currentGame) {
            return;
        }

        sendEvent(currentGame.id, new TimeoutClockSet(value));
    }

    return (
        <Clock
            seconds={clockState?.secondsPassed}
            isRunning={clockState?.isRunning ?? false}
            direction="up" 
            onClockSet={handleClockSet}
            {...props} 
        />
    );
}