import { useCurrentGame, useGameState } from "@/hooks";
import { Clock, ClockProps } from "./Clock";
import { useEvents } from "@/hooks/EventsApiHook";
import { IntermissionClockSet } from "@/types/events";

type IntermissionClockState = {
    isRunning: boolean;
    hasExpired: boolean;
    targetTick: number;
    secondsRemaining: number;
};

type IntermissionClockProps = Omit<ClockProps, "seconds" | "isRunning" | "direction" | "startValue">

export const IntermissionClock = (props: IntermissionClockProps) => {

    const clockState = useGameState<IntermissionClockState>("IntermissionClockState");
    const { currentGame } = useCurrentGame();

    const { sendEvent } = useEvents();

    const handleClockSet = (value: number) => {
        if(!currentGame) {
            return;
        }

        sendEvent(currentGame.id, new IntermissionClockSet(value));
    }

    return (
        <Clock 
            seconds={clockState?.secondsRemaining}
            isRunning={clockState?.isRunning ?? false}
            direction="up"
            onClockSet={handleClockSet}
            {...props} 
        />
    );
}