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
}

export const MainControls = ({ gameId }: MainControlsProps) => {

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
            case Stage.Intermission: return [translate("MainControls.FinalizePeriod"), !gameStage.periodIsFinalized];
            case Stage.AfterGame: return [translate("MainControls.FinalizeGame"), true];
            default: return ["---", false];
        }
    }, [gameStage?.stage, language]);

    const [timeoutText, timeoutButtonEnabled] = useMemo(() => {
        if(gameStage && !gameStage.periodIsFinalized) {
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

    return (
        <Card className="grow mt-5 pt-6">
            <CardContent className="flex flex-wrap gap-2 justify-evenly">
                <Button onClick={handleStart} disabled={!startButtonEnabled}><Play /> { startText } [`]</Button>
                <Button onClick={handleEnd} disabled={!endButtonEnabled}><Square /> { endText } [y]</Button>
                <Button onClick={handleTimeout} disabled={!timeoutButtonEnabled}><Pause /> { timeoutText } [t]</Button>
                <Button disabled={!undoButtonEnabled}><Undo /> {undoText} [g]</Button>
            </CardContent>
        </Card>
    );
}