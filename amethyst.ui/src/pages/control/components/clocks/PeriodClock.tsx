import { useCurrentGame, useEvents, useGameState } from "@/hooks";
import { Clock, ClockProps } from "./Clock";
import { PeriodClockState } from "@/types";
import { PeriodClockSet } from "@/types/events";

type PeriodClockProps = Omit<ClockProps, "seconds" | "isRunning" | "direction" | "startValue">;

export const PeriodClock = (props: PeriodClockProps) => {

    const clockState = useGameState<PeriodClockState>("PeriodClockState");
    const { currentGame } = useCurrentGame();

    const { sendEvent } = useEvents();

    const handleClockSet = (value: number) => {
        if(!currentGame) {
            return;
        }

        sendEvent(currentGame.id, new PeriodClockSet(value));
    }

    return (
        <Clock
            seconds={clockState?.secondsPassed} 
            isRunning={clockState?.isRunning ?? false}
            direction="down" 
            startValue={30 * 60}
            onClockSet={handleClockSet}
            {...props} />
    );
}