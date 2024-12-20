import { useCurrentGame, useEvents, useGameState } from "@/hooks";
import { Clock, ClockProps } from "./Clock";
import { LineupClockState } from "@/types";
import { LineupClockSet } from "@/types/events";

type LineupClockProps = Omit<ClockProps, "seconds" | "isRunning" | "direction" | "startValue">;

export const LineupClock = (props: LineupClockProps) => {

    const clockState = useGameState<LineupClockState>("LineupClockState");
    const { currentGame } = useCurrentGame();

    const { sendEvent } = useEvents();

    const handleClockSet = (value: number) => {
        if(!currentGame) {
            return;
        }

        sendEvent(currentGame.id, new LineupClockSet(value));
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