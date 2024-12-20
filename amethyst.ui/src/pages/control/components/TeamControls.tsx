import { Card, CardContent } from "@/components/ui/card"
import { useTeamDetailsState, useTripScoreState } from "@/hooks";
import { TeamSide } from "@/types"
import { useMemo } from "react";
import { TeamScore } from "./TeamScore";
import { Button } from "@/components/ui/button";
import { TripScore } from "./TripScore";
import { Event, useEvents } from "@/hooks/EventsApiHook";
import { ScoreModifiedRelative } from "@/types/events/Scores";
import { useHotkeys } from "react-hotkeys-hook";
import { JamScore } from "./JamScore";
import { SkaterOnTrack, SkaterPosition } from "@/types/events/JamLineup";
import { useUserSettings } from "@/hooks/UserSettings";
import { JamStats } from "./JamStats";
import { TeamLineup } from "./TeamLineup";
import { SeparatedCollection } from "@/components/SeparatedCollection";
import { CallMarked, InitialTripCompleted, LeadMarked, LostMarked, StarPassMarked } from "@/types/events/JamStats";
import { LastTripDeleted } from "@/types/events";

type TeamControlsProps = {
    gameId?: string;
    side: TeamSide;
    disabled?: boolean;
}

export const TeamControls = ({ gameId, side, disabled }: TeamControlsProps) => {

    const { sendEvent } = useEvents();

    const { userSettings } = useUserSettings();

    const team = useTeamDetailsState(side);
    const teamName = useMemo(() => {
        if(!team) {
            return '';
        }

        return team.team.names['controls'] || team.team.names['default'] || '';
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

    const handleLineupSelected = (position: SkaterPosition, number: string | null) =>
        sendEventIfIdSet(new SkaterOnTrack(side, position, number));

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

    const tripShortcutKeys: string[] = [];
    tripShortcutKeys[0] = side === TeamSide.Home ? "🠅a" : "🠅'";
    tripShortcutKeys[4] = side === TeamSide.Home ? "🠅s" : "🠅#";

    useHotkeys(side === TeamSide.Home ? 'a' : '\'', decrementScore, { preventDefault: true });
    useHotkeys(side === TeamSide.Home ? 's' : '#', incrementScore, { preventDefault: true });
    useHotkeys(side === TeamSide.Home ? 'shift+a' : 'shift+quote', () => setTripScore(0), { preventDefault: true });
    useHotkeys(side === TeamSide.Home ? 'shift+s' : 'shift+#', () => setTripScore(4), { preventDefault: true });

    return (
        <Card className="grow inline-block">
            <CardContent className="py-0">
                <SeparatedCollection>
                    { teamName && <div className="text-center text-xl">{teamName}</div> }
                    { userSettings.showScoreControls && (
                        <>
                            <div>
                                <JamScore side={side} />
                                <div className="flex w-full justify-center items-center">
                                    <Button onClick={decrementScore} variant="secondary" disabled={disabled} className="text-md lg:text-xl">-1 [{side === TeamSide.Home ? 'a' : '\''}]</Button>
                                    <TeamScore side={side} />
                                    <Button onClick={incrementScore} variant="secondary" disabled={disabled} className="text-md lg:text-xl" >+1 [{side === TeamSide.Home ? 's' : '#'}]</Button>
                                </div>
                            </div>
                            <TripScore tripScore={tripScore?.score ?? -1} disabled={disabled} scoreShortcutKeys={tripShortcutKeys} onTripScoreSet={setTripScore} />
                        </>
                    )}
                    { userSettings.showStatsControls && (
                        <>
                            <JamStats 
                                side={side} 
                                disabled={disabled} 
                                onLeadChanged={handleLeadChanged} 
                                onLostChanged={handleLostChanged} 
                                onCallChanged={handleCallChanged} 
                                onStarPassChanged={handleStarPassChanged}
                                onInitialPassChanged={handleInitialCompletedChanged}
                            />
                        </>
                    )}
                    { userSettings.showLineupControls && (
                        <TeamLineup side={side} disabled={disabled} onLineupSelected={handleLineupSelected} />
                    )}
                </SeparatedCollection>
            </CardContent>
        </Card>
    )
}