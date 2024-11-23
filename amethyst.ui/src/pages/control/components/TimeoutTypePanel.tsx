import { Button, Card, CardContent } from "@/components/ui";
import { useCurrentTimeoutTypeState, useGameStageState } from "@/hooks";
import { useEvents } from "@/hooks/EventsApiHook";
import { useI18n } from "@/hooks/I18nHook";
import { cn } from "@/lib/utils";
import { Stage, TeamSide, TimeoutType } from "@/types";
import { TimeoutTypeSet } from "@/types/events";
import { useMemo } from "react";

type TimeoutTypePanelProps = {
    gameId?: string;
}

type CompoundTimeoutType =
    "Untyped"
    | "HomeTeamTimeout"
    | "HomeTeamReview"
    | "AwayTeamTimeout"
    | "AwayTeamReview"
    | "Official";

export const TimeoutTypePanel = ({ gameId }: TimeoutTypePanelProps) => {

    const { translate } = useI18n();

    const timeoutType = useCurrentTimeoutTypeState();
    const { stage } = useGameStageState() ?? { stage: Stage.BeforeGame };

    const { sendEvent } = useEvents();
    const compoundType = useMemo<CompoundTimeoutType>(() =>
        timeoutType?.type === TimeoutType.Team && timeoutType?.side === TeamSide.Home ? 'HomeTeamTimeout'
        : timeoutType?.type === TimeoutType.Team && timeoutType?.side === TeamSide.Away ? 'AwayTeamTimeout'
        : timeoutType?.type === TimeoutType.Review && timeoutType?.side === TeamSide.Home ? 'HomeTeamReview'
        : timeoutType?.type === TimeoutType.Review && timeoutType?.side === TeamSide.Away ? 'AwayTeamReview'
        : timeoutType?.type === TimeoutType.Official ? 'Official'
        : 'Untyped'
    , [timeoutType]);

    const handleHomeTeamTimeout = () => {
        if (!gameId) return;

        if(compoundType === 'HomeTeamTimeout') {
            sendEvent(gameId, new TimeoutTypeSet({ type: "Untyped" }));
        } else {
            sendEvent(gameId, new TimeoutTypeSet({ type: "Team", side: TeamSide.Home }));
        }
    }

    const handleHomeTeamReview = () => {
        if (!gameId) return;

        if(compoundType === 'HomeTeamReview') {
            sendEvent(gameId, new TimeoutTypeSet({ type: "Untyped" }));
        } else {
            sendEvent(gameId, new TimeoutTypeSet({ type: "Review", side: TeamSide.Home }));
        }
    }

    const handleAwayTeamTimeout = () => {
        if (!gameId) return;

        if(compoundType === 'AwayTeamTimeout') {
            sendEvent(gameId, new TimeoutTypeSet({ type: "Untyped" }));
        } else {
            sendEvent(gameId, new TimeoutTypeSet({ type: "Team", side: TeamSide.Away }));
        }
    }

    const handleAwayTeamReview = () => {
        if (!gameId) return;

        if(compoundType === 'AwayTeamReview') {
            sendEvent(gameId, new TimeoutTypeSet({ type: "Untyped" }));
        } else {
            sendEvent(gameId, new TimeoutTypeSet({ type: "Review", side: TeamSide.Away }));
        }
    }

    const handleOfficialTimeout = () => {
        if (!gameId) return;

        if(compoundType === 'Official') {
            sendEvent(gameId, new TimeoutTypeSet({ type: "Untyped" }));
        } else {
            sendEvent(gameId, new TimeoutTypeSet({ type: "Official" }));
        }
    }
    console.log(stage);

    return (
        <Card className={cn("grow scale-0 transition-all duration-500 h-0 m-0 p-0", stage === Stage.Timeout ? "scale-100 block h-auto  mt-5 pt-6" : "")}>
            <CardContent className="flex flex-wrap gap-2 justify-evenly">
                <Button variant={compoundType === 'HomeTeamTimeout' ? 'default' : 'secondary'} onClick={handleHomeTeamTimeout}>{translate("TimeoutType.HomeTeam")}</Button>
                <Button variant={compoundType === 'HomeTeamReview' ? 'default' : 'secondary'} onClick={handleHomeTeamReview}>{translate("TimeoutType.HomeTeamReview")}</Button>
                <Button variant={compoundType === 'Official' ? 'default' : 'secondary'} onClick={handleOfficialTimeout}>{translate("TimeoutType.Official") }</Button>
                <Button variant={compoundType === 'AwayTeamTimeout' ? 'default' : 'secondary'} onClick={handleAwayTeamTimeout}>{translate("TimeoutType.AwayTeam")}</Button>
                <Button variant={compoundType === 'AwayTeamReview' ? 'default' : 'secondary'} onClick={handleAwayTeamReview}>{translate("TimeoutType.AwayTeamReview")}</Button>
            </CardContent>
        </Card>
    );
}