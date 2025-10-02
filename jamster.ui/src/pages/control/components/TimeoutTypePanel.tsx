import { RadioButtonGroup, TooltipRadioItem } from "@/components/RadioButtonGroup";
import { Card, CardContent } from "@/components/ui";
import { useCurrentTimeoutTypeState, useGameStageState, useTeamDetailsState } from "@/hooks";
import { useEvents } from "@/hooks/EventsApiHook";
import { useI18n } from "@/hooks/I18nHook";
import { cn } from "@/lib/utils";
import { Stage, TeamDetailsState, TeamSide, TimeoutType } from "@/types";
import { TimeoutTypeSet, TimeoutTypeSetBody } from "@/types/events";
import { switchex } from "@/utilities/switchex";
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

    const homeTeam = useTeamDetailsState(TeamSide.Home);
    const awayTeam = useTeamDetailsState(TeamSide.Away);

    const getTeamName = (team: TeamDetailsState | undefined, defaultName: string) => {
        if(!team) {
            return defaultName;
        }

        return team.team.names['controls'] || team.team.names['color'] || defaultName;
    }

    const homeTeamName = useMemo(() => getTeamName(homeTeam, "Home"), [homeTeam]);
    const awayTeamName = useMemo(() => getTeamName(awayTeam, "Away"), [awayTeam]);

    const compoundType = useMemo<CompoundTimeoutType>(() =>
        switchex(timeoutType?.type)
            .case(TimeoutType.Team).when(() => timeoutType?.teamSide === TeamSide.Home).then<CompoundTimeoutType>('HomeTeamTimeout')
            .case(TimeoutType.Team).when(() => timeoutType?.teamSide === TeamSide.Away).then('AwayTeamTimeout')
            .case(TimeoutType.Review).when(() => timeoutType?.teamSide === TeamSide.Home).then('HomeTeamReview')
            .case(TimeoutType.Review).when(() => timeoutType?.teamSide === TeamSide.Away).then('AwayTeamReview')
            .case(TimeoutType.Official).then('Official')
            .default('Untyped')
    , [timeoutType]);

    const timeoutTypes: TooltipRadioItem<CompoundTimeoutType>[] = [
        { 
            id: 'ScoreboardControl.TimeoutTypePanel.HomeTeamTimeout',
            value: 'HomeTeamTimeout', 
            name: translate("TimeoutType.Team").replace("{teamName}", homeTeamName), 
            description: translate("TimeoutType.Team.Description").replace("{teamName}", homeTeamName),
        },
        { 
            id: 'ScoreboardControl.TimeoutTypePanel.HomeTeamReview',
            value: 'HomeTeamReview', 
            name: translate("TimeoutType.TeamReview").replace("{teamName}", homeTeamName), 
            description: translate("TimeoutType.TeamReview.Description").replace("{teamName}", homeTeamName),
        },
        { 
            id: 'ScoreboardControl.TimeoutTypePanel.Official',
            value: 'Official', 
            name: translate("TimeoutType.Official"), 
            description: translate("TimeoutType.Official.Description"),
        },
        { 
            id: 'ScoreboardControl.TimeoutTypePanel.AwayTeamReview',
            value: 'AwayTeamReview', 
            name: translate("TimeoutType.TeamReview").replace("{teamName}", awayTeamName), 
            description: translate("TimeoutType.TeamReview.Description").replace("{teamName}", awayTeamName), 
        },
        {
            id: 'ScoreboardControl.TimeoutTypePanel.AwayTeamTimeout',
            value: 'AwayTeamTimeout', 
            name: translate("TimeoutType.Team").replace("{teamName}", awayTeamName), 
            description: translate("TimeoutType.Team.Description").replace("{teamName}", awayTeamName),
        },
    ];

    const handleTimeoutSelected = (selectedType: CompoundTimeoutType) => {
        if(!gameId) {
            return;
        }
        
        sendEvent(
            gameId, 
            new TimeoutTypeSet(
                switchex(selectedType)
                    .case('HomeTeamTimeout').then<TimeoutTypeSetBody>({ type: 'Team', teamSide: TeamSide.Home })
                    .case('HomeTeamReview').then({ type: 'Review', teamSide: TeamSide.Home })
                    .case('Official').then({ type: 'Official' })
                    .case('AwayTeamTimeout').then({ type: 'Team', teamSide: TeamSide.Away })
                    .case('AwayTeamReview').then({ type: 'Review', teamSide: TeamSide.Away })
                    .default({ type: 'Untyped' })
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