import { RadioButtonGroup, RadioItem } from "@/components/RadioButtonGroup";
import { Card, CardContent } from "@/components/ui";
import { useCurrentTimeoutTypeState, useGameStageState } from "@/hooks";
import { useEvents } from "@/hooks/EventsApiHook";
import { useI18n } from "@/hooks/I18nHook";
import { cn } from "@/lib/utils";
import { Stage, TeamSide, TimeoutType } from "@/types";
import { TimeoutTypeSet } from "@/types/events";
import { useMemo } from "react";

type TimeoutTypePanelProps = {
    gameId?: string;
    disabled?: boolean;
}

type CompoundTimeoutType =
    "Untyped"
    | "HomeTeamTimeout"
    | "HomeTeamReview"
    | "AwayTeamTimeout"
    | "AwayTeamReview"
    | "Official";

export const TimeoutTypePanel = ({ gameId, disabled }: TimeoutTypePanelProps) => {

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

    const timeoutTypes: RadioItem<CompoundTimeoutType>[] = [
        { value: 'HomeTeamTimeout', name: translate("TimeoutType.HomeTeam") },
        { value: 'HomeTeamReview', name: translate("TimeoutType.HomeTeamReview") },
        { value: 'Official', name: translate("TimeoutType.Official") },
        { value: 'AwayTeamTimeout', name: translate("TimeoutType.AwayTeam") },
        { value: 'AwayTeamTimeout', name: translate("TimeoutType.AwayTeamReview") },
    ];

    const handleTimeoutSelected = (selectedType: CompoundTimeoutType) => {
        if(!gameId) {
            return;
        }
        
        sendEvent(
            gameId, 
            new TimeoutTypeSet(
                selectedType === 'HomeTeamTimeout' ? { type: 'Team', side: TeamSide.Home }
                : selectedType === 'HomeTeamReview' ? { type: 'Review', side: TeamSide.Home }
                : selectedType === 'Official' ? { type: 'Official' }
                : selectedType === 'AwayTeamTimeout' ? { type: 'Team', side: TeamSide.Away }
                : selectedType === 'AwayTeamReview' ? { type: 'Review', side: TeamSide.Away }
                : { type: 'Untyped' }
            ));
    }

    const handleTimeoutDeselected = () => {
        if(!gameId) {
            return;
        }

        sendEvent(gameId, new TimeoutTypeSet({ type: "Untyped" }));
    }

    return (
        <Card className={cn("grow scale-0 transition-all duration-500 h-0 m-0 -mb-2 p-0", stage === Stage.Timeout ? "scale-100 block h-auto mb-0 p-0" : "")}>
            <CardContent className="p-2">
                <RadioButtonGroup
                    items={timeoutTypes}
                    value={compoundType}
                    toggle
                    rowClassName="justify-evenly"
                    disabled={disabled}
                    onItemSelected={handleTimeoutSelected}
                    onItemDeselected={handleTimeoutDeselected}
                />
            </CardContent>
        </Card>
    );
}