import { useCurrentGame, useEvents, useGameState } from "@/hooks";
import { Clock, ClockProps } from "./Clock";
import { JamClockState } from "@/types";
import { JamClockSet } from "@/types/events";

type JamClockProps = Omit<ClockProps, "seconds" | "isRunning" | "direction" | "startValue">;

export const JamClock = (props: JamClockProps) => {

    const clockState = useGameState<JamClockState>("JamClockState");
    const { currentGame } = useCurrentGame();

    const { sendEvent } = useEvents();

    const handleClockSet = (value: number) => {
        if(!currentGame) {
            return;
        }

        sendEvent(currentGame.id, new JamClockSet(value));
    }

    return (
        <Clock
            seconds={clockState?.secondsPassed} 
            isRunning={clockState?.isRunning ?? false}
            direction="down" 
            startValue={120} 
            onClockSet={handleClockSet}
            {...props} 
        />
    );
}