import { ShortcutButton } from "@/components";
import { TooltipProvider } from "@/components/ui";
import { Card, CardContent } from "@/components/ui/card";
import { useGameStageState, useLineupClockState, useRulesState, useUndoListState } from "@/hooks";
import { Event, useEvents } from "@/hooks/EventsApiHook";
import { useI18n } from "@/hooks/I18nHook";
import { Stage } from "@/types";
import { IntermissionEnded, JamEnded, JamStarted, PeriodFinalized, TimeoutEnded, TimeoutStarted } from "@/types/events";
import { Pause, Play, Square, Undo } from "lucide-react";
import { useMemo } from "react";

type MainControlsProps = {
    gameId?: string;
    disabled?: boolean;
}

export const MainControls = ({ gameId, disabled }: MainControlsProps) => {

    const gameStage = useGameStageState();
    const {translate, language} = useI18n({ prefix: "ScoreboardControl.MainControls." });
    const { sendEvent, deleteEvent } = useEvents();
    const undoList = useUndoListState() ?? { };
    const lineupClock = useLineupClockState();
    const { rules } = useRulesState() ?? { };

    const [startText, startDescription, startButtonEnabled] = useMemo(() => {
        if (!gameStage || (gameStage.stage === Stage.AfterGame && gameStage.periodIsFinalized) || gameStage.stage === Stage.Jam) {
            return ["---", "", false];
        } else {
            return [translate("StartJam"), translate("StartJam.Description"), true];
        }
    }, [gameStage, language]);

    const [endText, endDescription, endButtonEnabled] = useMemo(() => {
        switch (gameStage?.stage) {
            case Stage.Jam: 
                return [translate("EndJam"), translate("EndJam.Description"), true];
            case Stage.Timeout: 
                return [translate("EndTimeout"), translate("EndTimeout.Description"), true];
            case Stage.BeforeGame:
                return [translate("StartLineup"), translate("StartLineup.Description"), true];
            case Stage.Intermission: 
                return gameStage.periodIsFinalized ? [translate("StartLineup"), translate("StartLineup.Description"), true] : [translate("FinalizePeriod"), translate("FinalizePeriod.Description"), true];
            case Stage.AfterGame: 
                return gameStage.periodIsFinalized ? ["---", "", false] : [translate("FinalizeGame"), translate("FinalizeGame.Description"), true];
            default: 
                return ["---", "", false];
        }
    }, [gameStage?.stage, gameStage?.periodIsFinalized, language]);

    const [timeoutText, timeoutDescription, timeoutButtonEnabled] = useMemo(() => {
        if(gameStage && !gameStage.periodIsFinalized && gameStage.stage !== Stage.BeforeGame) {
            return [translate("NewTimeout"), translate("NewTimeout.Description"), true];
        } else {
            return ["---", "", false];
        }
    }, [gameStage, language]);

    const [undoText, undoDescription, undoButtonEnabled] = useMemo(() => {
        return undoList.latestUndoEventId
            ? [
                `${translate("Undo")} ${translate(`Undo.${undoList.latestUndoEventName}`)}`, 
                translate(`Undo.${undoList.latestUndoEventName}.Description`),
                true
            ]
            : [translate("Undo"), "", false];
    }, [language, undoList]);

    const sendEventIfIdSet = (event: Event) => {
        if(!gameId) {
            return;
        }

        sendEvent(gameId, event);
    }

    const handleStart = () => {
        sendEventIfIdSet(new JamStarted());
    };

    const handleEnd = () => {
        if(gameStage?.stage === Stage.Jam) {
            sendEventIfIdSet(new JamEnded());
        } else if(gameStage?.stage === Stage.Timeout) {
            sendEventIfIdSet(new TimeoutEnded());
        } else if((gameStage?.stage === Stage.Intermission || gameStage?.stage === Stage.AfterGame) && !gameStage.periodIsFinalized) {
            sendEventIfIdSet(new PeriodFinalized());
        } else if(gameStage?.stage === Stage.BeforeGame || gameStage?.stage === Stage.Intermission) {
            sendEventIfIdSet(new IntermissionEnded());
        }
    }

    const handleTimeout = () => {
        sendEventIfIdSet(new TimeoutStarted());
    }

    const handleUndo = () => {
        if(!undoList.latestUndoEventId || !gameId) {
            return;
        }

        deleteEvent(gameId, undoList.latestUndoEventId);
    }

    const buttonClass = "w-full py-6 md:w-auto md:px-4 md:py-2";

    const shouldStartJam = gameStage?.stage === Stage.Lineup && (lineupClock?.secondsPassed ?? 0) > (rules?.lineupRules.durationInSeconds ?? 0);
    const shouldFinalizePeriod = gameStage && [Stage.Intermission, Stage.AfterGame].includes(gameStage.stage) && !gameStage.periodIsFinalized;

    return (
        <Card className="grow py-2">
            <CardContent className="flex p-0 px-2 flex-wrap gap-2 justify-evenly">
                <TooltipProvider>
                    <ShortcutButton 
                        shortcutGroup="clocks" 
                        shortcutKey="start"
                        notify={shouldStartJam}
                        description={startDescription}
                        className={buttonClass} 
                        variant={startButtonEnabled ? 'default' : 'secondary'} 
                        disabled={disabled || !startButtonEnabled}
                        onClick={handleStart} 
                    >
                        <Play />
                        { startText }
                    </ShortcutButton>
                    <ShortcutButton
                        shortcutGroup="clocks"
                        shortcutKey="stop"
                        notify={shouldFinalizePeriod}
                        description={endDescription}
                        className={buttonClass}
                        variant={endButtonEnabled ? "default" : "secondary"}
                        disabled={disabled || !endButtonEnabled}
                        onClick={handleEnd}
                    >
                        <Square />
                        { endText }
                    </ShortcutButton>
                    <ShortcutButton
                        shortcutGroup="clocks"
                        shortcutKey="timeout"
                        notify={shouldStartJam}
                        description={timeoutDescription}
                        className={buttonClass}
                        variant={timeoutButtonEnabled ? "default" : "secondary"}
                        disabled={disabled || !timeoutButtonEnabled}
                        onClick={handleTimeout}
                    >
                        <Pause />
                        { timeoutText }
                    </ShortcutButton>
                    <ShortcutButton
                        shortcutGroup="clocks"
                        shortcutKey="undo"
                        description={undoDescription}
                        className={buttonClass}
                        variant={undoButtonEnabled ? "default" : "secondary"}
                        disabled={disabled || !undoButtonEnabled}
                        onClick={handleUndo}
                    >
                        <Undo />
                        { undoText }
                    </ShortcutButton>
                </TooltipProvider>
            </CardContent>
        </Card>
    );
}