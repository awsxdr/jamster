import { useEvents, useGameStageState, useIntermissionClockState } from "@/hooks"
import { useI18n } from "@/hooks/I18nHook";
import { Button } from "@/components/ui";
import { Pencil } from "lucide-react";
import { useState } from "react";
import { cn } from "@/lib/utils";
import { IntermissionClock, JamClock, LineupClock, PeriodClock, TimeoutClock } from "./clocks";
import { IntermissionClockSet, IntermissionStarted, JamClockSet, LineupClockSet, PeriodClockSet, TimeoutClockSet } from "@/types/events";
import { Stage } from "@/types";

type ClocksContainerProps = {
    gameId: string;
}

export const ClocksContainer = ({ gameId }: ClocksContainerProps) => {
    const gameStage = useGameStageState();
    const { translate } = useI18n();
    const { sendEvent } = useEvents();
    const intermissionClock = useIntermissionClockState();

    const [isEditing, setIsEditing] = useState(false);

    const handleEditClicked = () => {
        setIsEditing(v => !v);
    }

    const handlePeriodClockSet = (value: number) =>
        sendEvent(gameId, new PeriodClockSet(value));

    const handleJamClockSet = (value: number) =>
        sendEvent(gameId, new JamClockSet(value));

    const handleLineupClockSet = (value: number) =>
        sendEvent(gameId, new LineupClockSet(value));

    const handleTimeoutClockSet = (value: number) =>
        sendEvent(gameId, new TimeoutClockSet(value));

    const handleIntermissionClockSet = (value: number) => {
        sendEvent(gameId, new IntermissionClockSet(value));

        if(!intermissionClock?.isRunning && gameStage?.stage && [Stage.BeforeGame, Stage.Intermission, Stage.AfterGame].includes(gameStage.stage)) {
            sendEvent(gameId, new IntermissionStarted());
        }
    }

    return (
        <div className="w-full flex gap-2 items-center">
            <div className="w-full grid grid-flow-rows auto-cols-fr gap-2">
                <PeriodClock
                    className="col-start-1"
                    editing={isEditing} 
                    name={`${translate('ClocksContainer.Period')} ${gameStage?.periodNumber ?? 0}`} 
                    onClockSet={handlePeriodClockSet}
                />
                <JamClock
                    className="col-start-2"
                    editing={isEditing} 
                    name={`${translate('ClocksContainer.Jam')} ${gameStage?.jamNumber ?? 0}`} 
                    onClockSet={handleJamClockSet}
                />
                <LineupClock
                    className="col-start-3"
                    editing={isEditing}
                    name={translate('ClocksContainer.Lineup')}
                    onClockSet={handleLineupClockSet}
                />
                <TimeoutClock 
                    className="col-start-1 lg:col-start-4"
                    editing={isEditing} 
                    name={translate('ClocksContainer.Timeout')} 
                    onClockSet={handleTimeoutClockSet}
                />
                <IntermissionClock
                    className="col-start-2 lg:col-start-5"
                    editing={isEditing} 
                    name={translate('ClocksContainer.Intermission')} 
                    onClockSet={handleIntermissionClockSet}
                />
            </div>
            <Button 
                size="icon" 
                variant="ghost" 
                className={cn("border-2 border-transparent", isEditing && "border-primary")} 
                onClick={handleEditClicked}
            >
                <Pencil />
            </Button>
        </div>
    )
}