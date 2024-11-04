import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useGameStageState } from "@/hooks";
import { Event, useEvents } from "@/hooks/EventsApiHook";
import { useI18n } from "@/hooks/I18nHook";
import { Stage } from "@/types";
import { JamEnded, JamStarted, PeriodFinalized, TimeoutEnded, TimeoutStarted } from "@/types/events";
import { Pause, Play, Square, Undo } from "lucide-react";
import { useMemo } from "react";

type MainControlsProps = {
    gameId?: string;
}

export const MainControls = ({ gameId }: MainControlsProps) => {

    const gameStage = useGameStageState();
    const {translate} = useI18n();
    const { sendEvent } = useEvents();

    const [startText, startButtonEnabled] = useMemo(() => {
        if (!gameStage || gameStage.periodIsFinalized || gameStage.stage === Stage.Jam) {
            return ["---", false];
        } else {
            return [translate("Start jam"), true];
        }
    }, [gameStage]);

    const [endText, endButtonEnabled] = useMemo(() => {
        switch (gameStage?.stage) {
            case Stage.Jam: return [translate("End jam"), true];
            case Stage.Timeout: return [translate("End timeout"), true];
            case Stage.Intermission: return [translate("Finalize period"), true];
            case Stage.AfterGame: return [translate("Finalize game"), true];
            default: return ["---", false];
        }
    }, [gameStage?.stage]);

    const [timeoutText, timeoutButtonEnabled] = useMemo(() => {
        if(gameStage && !gameStage.periodIsFinalized) {
            return [translate("New timeout"), true];
        } else {
            return ["---", false];
        }
    }, [gameStage]);

    const [undoText, undoButtonEnabled] = useMemo(() => {
        return [translate("Undo"), true];
    }, []);

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

    return (
        <Card className="grow m-2 pt-6">
            <CardContent className="flex justify-evenly">
                <Button onClick={handleStart} disabled={!startButtonEnabled}><Play /> { startText} [`]</Button>
                <Button onClick={handleEnd} disabled={!endButtonEnabled}><Square /> { endText } [y]</Button>
                <Button onClick={handleTimeout} disabled={!timeoutButtonEnabled}><Pause /> {timeoutText} [t]</Button>
                <Button disabled={!undoButtonEnabled}><Undo /> {undoText} [g]</Button>
            </CardContent>
        </Card>
    );
}