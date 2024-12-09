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
import { TripStats } from "./TripStats";
import { TeamLineup } from "./TeamLineup";
import { SeparatedCollection } from "@/components/SeparatedCollection";

type TeamControlsProps = {
    gameId?: string;
    side: TeamSide;
    disabled?: boolean;
}

export const TeamControls = ({ gameId, side, disabled }: TeamControlsProps) => {

    const { sendEvent } = useEvents();

    const userSettings = useUserSettings();

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
        const scoreDelta = score - (tripScore?.score ?? 0);
        sendEventIfIdSet(new ScoreModifiedRelative({ teamSide: side, value: scoreDelta }));
    }

    const handleLineupSelected = (position: SkaterPosition, number: string | null) =>
        sendEventIfIdSet(new SkaterOnTrack(side, position, number));

    const tripShortcutKeys: string[] = [];
    tripShortcutKeys[4] = side === TeamSide.Home ? "🠅s" : "🠅#";

    useHotkeys(side === TeamSide.Home ? 'a' : '\'', decrementScore, { preventDefault: true });
    useHotkeys(side === TeamSide.Home ? 's' : '#', incrementScore, { preventDefault: true });
    useHotkeys(side === TeamSide.Home ? 'shift+s' : 'shift+#', () => setTripScore(4), { preventDefault: true });

    return (
        <Card className="grow inline-block mt-5">
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
                            <TripScore tripScore={tripScore?.score ?? 0} disabled={disabled} scoreShortcutKeys={tripShortcutKeys} onTripScoreSet={setTripScore} />
                        </>
                    )}
                    { userSettings.showStatsControls && (
                        <>
                            <TripStats side={side} disabled={disabled} />
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