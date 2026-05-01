import { useCurrentUserConfiguration, useEvents, useTeamDetailsState, useTripScoreState, Event, useI18n } from "@/hooks";
import { ControlPanelViewConfiguration, DEFAULT_CONTROL_PANEL_VIEW_CONFIGURATION, TeamSide } from "@/types"
import { useMemo } from "react";
import { TeamScore } from "./TeamScore";
import { TripScore } from "./TripScore";
import { JamScore } from "./JamScore";
import { JamStats } from "./JamStats";
import { TeamLineup } from "./TeamLineup";
import { CallMarked, InitialTripCompleted, LastTripDeleted, LeadMarked, LostMarked, ScoreModifiedRelative, SkaterOffTrack, SkaterOnTrack, SkaterPosition, StarPassMarked } from "@/types/events";
import { cn } from "@/lib/utils";
import { Card, CardContent } from "@/components/ui";
import { SeparatedCollection, ShortcutButton } from "@/components";

type TeamControlsProps = {
    gameId?: string;
    side: TeamSide;
    disabled?: boolean | 'allExceptLineup';
    className?: string;
}

export const TeamControls = ({ gameId, side, disabled, className }: TeamControlsProps) => {

    const { sendEvent } = useEvents();

    const { translate } = useI18n({ prefix: "ScoreboardControl.TeamControls." })

    const { configuration: viewConfiguration } = useCurrentUserConfiguration<ControlPanelViewConfiguration>("ControlPanelViewConfiguration", DEFAULT_CONTROL_PANEL_VIEW_CONFIGURATION);

    const team = useTeamDetailsState(side);
    const teamName = useMemo(() => {
        if(!team) {
            return '';
        }

        return team.team.names['controls'] || team.team.names['color'] ||  team.team.names['team'] || team.team.names['league'] || '';
    }, [team]);

    const tripScore = useTripScoreState(side);

    const sendEventIfIdSet = (event: Event) => {
        if(!gameId) return;

        sendEvent(gameId, event);
    }

    const incrementScore = () => {
        if(disabled) {
            return;
        }
        sendEventIfIdSet(new ScoreModifiedRelative({ teamSide: side, value: 1 }));
    }

    const decrementScore = () => {
        if(disabled) {
            return;
        }
        sendEventIfIdSet(new ScoreModifiedRelative({ teamSide: side, value: -1 }));
    }

    const setTripScore = (score: number) => {
        if(disabled) {
            return;
        }

        if (score === -1) { // -1 signals clearing the trip score
            sendEventIfIdSet(new LastTripDeleted(side));
        } else {
            const scoreDelta = score - (tripScore?.score ?? 0);
            sendEventIfIdSet(new ScoreModifiedRelative({ teamSide: side, value: scoreDelta }));
        }
    }

    const handleLineupSelected = (position: SkaterPosition, number: string | null, currentNumber: string | undefined) => {
        if(number === null) {
            if(currentNumber !== undefined) {
                sendEventIfIdSet(new SkaterOffTrack(side, currentNumber))
            }
        } else {
            sendEventIfIdSet(new SkaterOnTrack(side, position, number));
        }
    }

    const handleLeadChanged = (side: TeamSide, value: boolean) => {
        sendEventIfIdSet(new LeadMarked(side, value));
    }

    const handleLostChanged = (side: TeamSide, value: boolean) => {
        sendEventIfIdSet(new LostMarked(side, value));
    }

    const handleCallChanged = (side: TeamSide, value: boolean) => {
        sendEventIfIdSet(new CallMarked(side, value));
    }

    const handleStarPassChanged = (side: TeamSide, value: boolean) => {
        sendEventIfIdSet(new StarPassMarked(side, value));
    }

    const handleInitialCompletedChanged = (side: TeamSide, value: boolean) => {
        sendEventIfIdSet(new InitialTripCompleted(side, value));
    }

    return (
        <Card className={cn("w-full inline-block", className)}>
            <CardContent className="p-0">
                <SeparatedCollection>
                    { teamName && <div className="text-center text-xl">{teamName}</div> }
                    { viewConfiguration.showScoreControls && (
                        <>
                            <div>
                                <JamScore side={side} />
                                <div className="flex w-full justify-center items-center">
                                    <ShortcutButton 
                                        shortcutGroup={side === TeamSide.Home ? "homeScore" : "awayScore"}
                                        shortcutKey="decrementScore"
                                        description={translate("Decrement").replace("{team}", teamName)} 
                                        onClick={decrementScore} 
                                        variant="secondary" 
                                        disabled={!!disabled} 
                                        className="text-md lg:text-xl"
                                    >
                                        -1
                                    </ShortcutButton>
                                    <TeamScore side={side} />
                                    <ShortcutButton
                                        shortcutGroup={side === TeamSide.Home ? "homeScore" : "awayScore"}
                                        shortcutKey="incrementScore"
                                        description={translate("Increment").replace("{team}", teamName)} 
                                        onClick={incrementScore} variant="secondary" 
                                        disabled={!!disabled} 
                                        className="text-md lg:text-xl"
                                    >
                                        +1
                                    </ShortcutButton>
                                </div>
                            </div>
                            <TripScore teamSide={side} tripScore={tripScore?.score ?? -1} disabled={!!disabled} onTripScoreSet={setTripScore} />
                        </>
                    )}
                    { viewConfiguration.showStatsControls && (
                        <>
                            <JamStats 
                                side={side} 
                                disabled={!!disabled} 
                                onLeadChanged={handleLeadChanged} 
                                onLostChanged={handleLostChanged} 
                                onCallChanged={handleCallChanged} 
                                onStarPassChanged={handleStarPassChanged}
                                onInitialPassChanged={handleInitialCompletedChanged}
                            />
                        </>
                    )}
                    { viewConfiguration.showLineupControls && (team?.team.roster.length ?? 0) > 0 && (
                        <TeamLineup side={side} disabled={disabled === true} onLineupSelected={handleLineupSelected} />
                    )}
                </SeparatedCollection>
            </CardContent>
        </Card>
    )
}