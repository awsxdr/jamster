import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useGameStageState } from "@/hooks";
import { Event, useEvents } from "@/hooks/EventsApiHook";
import { useI18n } from "@/hooks/I18nHook";
import { Stage } from "@/types";
import { JamEnded, JamStarted, PeriodFinalized, TimeoutEnded, TimeoutStarted } from "@/types/events";
import { Pause, Play, Square, Undo } from "lucide-react";
import { useMemo } from "react";
import { useHotkeys } from 'react-hotkeys-hook';

type MainControlsProps = {
    gameId?: string;
    disabled?: boolean;
}

export const MainControls = ({ gameId, disabled }: MainControlsProps) => {

    const gameStage = useGameStageState();
    const {translate, language} = useI18n();
    const { sendEvent } = useEvents();

    const [startText, startButtonEnabled] = useMemo(() => {
        if (!gameStage || (gameStage.stage === Stage.AfterGame && gameStage.periodIsFinalized) || gameStage.stage === Stage.Jam) {
            return ["---", false];
        } else {
            return [translate("MainControls.StartJam"), true];
        }
    }, [gameStage, language]);

    const [endText, endButtonEnabled] = useMemo(() => {
        switch (gameStage?.stage) {
            case Stage.Jam: return [translate("MainControls.EndJam"), true];
            case Stage.Timeout: return [translate("MainControls.EndTimeout"), true];
            case Stage.BeforeGame: return [translate("MainControls.StartLineup"), true];
            case Stage.Intermission: return [translate("MainControls.FinalizePeriod"), !gameStage.periodIsFinalized];
            case Stage.AfterGame: return [translate("MainControls.FinalizeGame"), !gameStage.periodIsFinalized];
            default: return ["---", false];
        }
    }, [gameStage?.stage, language]);

    const [timeoutText, timeoutButtonEnabled] = useMemo(() => {
        if(gameStage && !gameStage.periodIsFinalized && gameStage.stage !== Stage.BeforeGame) {
            return [translate("MainControls.NewTimeout"), true];
        } else {
            return ["---", false];
        }
    }, [gameStage, language]);

    const [undoText, undoButtonEnabled] = useMemo(() => {
        return [translate("MainControls.Undo"), false];
    }, [language]);

    const sendEventIfIdSet = (event: Event) => {
        if(!gameId) return;

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

    useHotkeys('`', handleStart);
    useHotkeys('y', handleEnd);
    useHotkeys('t', handleTimeout);

    const buttonClass = "w-full py-6 md:w-auto md:px-4 md:py-2";

    return (
        <Card className="grow py-2">
            <CardContent className="flex p-0 px-2 flex-wrap gap-2 justify-evenly">
                <Button className={buttonClass} onClick={handleStart} variant={startButtonEnabled ? 'default' : 'secondary'} disabled={disabled || !startButtonEnabled}><Play /> { startText } [`]</Button>
                <Button className={buttonClass} onClick={handleEnd} variant={endButtonEnabled ? 'default' : 'secondary'} disabled={disabled || !endButtonEnabled}><Square /> { endText } [y]</Button>
                <Button className={buttonClass} onClick={handleTimeout} variant={timeoutButtonEnabled ? 'default' : 'secondary'} disabled={disabled || !timeoutButtonEnabled}><Pause /> { timeoutText } [t]</Button>
                <Button className={buttonClass} variant={undoButtonEnabled ? 'default' : 'secondary'} disabled={disabled || !undoButtonEnabled}><Undo /> {undoText} [g]</Button>
            </CardContent>
        </Card>
    );
}