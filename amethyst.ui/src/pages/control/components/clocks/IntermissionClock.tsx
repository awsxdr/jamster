import { useCurrentGame, useGameStageState, useGameState, useIntermissionClockState } from "@/hooks";
import { Clock, ClockProps } from "./Clock";
import { useEvents } from "@/hooks/EventsApiHook";
import { IntermissionClockSet } from "@/types/events";
import { IntermissionClockState } from "@/types/IntermissionClockState";
import { Stage } from "@/types";
import { IntermissionStarted } from "@/types/events/Intermission";

type IntermissionClockProps = Omit<ClockProps, "seconds" | "isRunning" | "direction" | "startValue">

export const IntermissionClock = (props: IntermissionClockProps) => {

    const clockState = useGameState<IntermissionClockState>("IntermissionClockState");
    const { currentGame } = useCurrentGame();
    const { stage } = useGameStageState() ?? { };
    const intermissionClock = useIntermissionClockState();

    const { sendEvent } = useEvents();

    const handleClockSet = (value: number) => {
        if(!currentGame) {
            return;
        }

        sendEvent(currentGame.id, new IntermissionClockSet(value));

        if(!intermissionClock?.isRunning && stage && [Stage.BeforeGame, Stage.Intermission, Stage.AfterGame].includes(stage)) {
            sendEvent(currentGame.id, new IntermissionStarted());
        }
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