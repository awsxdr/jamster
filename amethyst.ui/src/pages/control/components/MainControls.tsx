import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useGameStageState, useUndoListState } from "@/hooks";
import { Event, useEvents } from "@/hooks/EventsApiHook";
import { useI18n } from "@/hooks/I18nHook";
import { useShortcut } from "@/hooks/InputControls";
import { Stage } from "@/types";
import { JamEnded, JamStarted, PeriodFinalized, TimeoutEnded, TimeoutStarted } from "@/types/events";
import { Pause, Play, Square, Undo } from "lucide-react";
import { useMemo } from "react";

type MainControlsProps = {
    gameId?: string;
    disabled?: boolean;
}

export const MainControls = ({ gameId, disabled }: MainControlsProps) => {

    const gameStage = useGameStageState();
    const {translate, language} = useI18n();
    const { sendEvent, deleteEvent } = useEvents();
    const undoList = useUndoListState() ?? { };

    const [startText, startButtonEnabled] = useMemo(() => {
        if (!gameStage || (gameStage.stage === Stage.AfterGame && gameStage.periodIsFinalized) || gameStage.stage === Stage.Jam) {
            return ["---", false];
        } else {
            return [translate("MainControls.StartJam"), true];
        }
    }, [gameStage, language]);

    const [endText, endButtonEnabled] = useMemo(() => {
        switch (gameStage?.stage) {
            case Stage.Jam: 
                return [translate("MainControls.EndJam"), true];
            case Stage.Timeout: 
                return [translate("MainControls.EndTimeout"), true];
            case Stage.Intermission: 
                return gameStage.periodIsFinalized ? ["---", false] : [translate("MainControls.FinalizePeriod"), true];
            case Stage.AfterGame: 
                return gameStage.periodIsFinalized ? ["---", false] : [translate("MainControls.FinalizeGame"), true];
            default: 
                return ["---", false];
        }
    }, [gameStage?.stage, gameStage?.periodIsFinalized, language]);

    const [timeoutText, timeoutButtonEnabled] = useMemo(() => {
        if(gameStage && !gameStage.periodIsFinalized && gameStage.stage !== Stage.BeforeGame) {
            return [translate("MainControls.NewTimeout"), true];
        } else {
            return ["---", false];
        }
    }, [gameStage, language]);

    const [undoText, undoButtonEnabled] = useMemo(() => {
        return undoList.latestUndoEventId
            ? [`${translate("MainControls.Undo")} ${translate(`MainControls.Undo.${undoList.latestUndoEventName}`)}`, true]
            : [translate("MainControls.Undo"), false];
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
        } else if(gameStage?.stage === Stage.Intermission || gameStage?.stage === Stage.AfterGame) {
            sendEventIfIdSet(new PeriodFinalized());
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

    useShortcut("clocks", "start", handleStart);
    useShortcut("clocks", "stop", handleEnd);
    useShortcut("clocks", "timeout", handleTimeout);
    useShortcut("clocks", "undo", handleUndo);

    const buttonClass = "w-full py-6 md:w-auto md:px-4 md:py-2";

    return (
        <Card className="grow py-2">
            <CardContent className="flex p-0 px-2 flex-wrap gap-2 justify-evenly">
                <Button className={buttonClass} onClick={handleStart} variant={startButtonEnabled ? 'default' : 'secondary'} disabled={disabled || !startButtonEnabled}><Play /> { startText }</Button>
                <Button className={buttonClass} onClick={handleEnd} variant={endButtonEnabled ? 'default' : 'secondary'} disabled={disabled || !endButtonEnabled}><Square /> { endText }</Button>
                <Button className={buttonClass} onClick={handleTimeout} variant={timeoutButtonEnabled ? 'default' : 'secondary'} disabled={disabled || !timeoutButtonEnabled}><Pause /> { timeoutText }</Button>
                <Button className={buttonClass} onClick={handleUndo} variant={undoButtonEnabled ? 'default' : 'secondary'} disabled={disabled || !undoButtonEnabled}><Undo /> {undoText}</Button>
            </CardContent>
        </Card>
    );
}