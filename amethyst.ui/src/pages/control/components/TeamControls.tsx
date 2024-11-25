import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useTeamDetailsState, useTripScoreState } from "@/hooks";
import { TeamSide } from "@/types"
import { useMemo } from "react";
import { TeamScore } from "./TeamScore";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { TripScore } from "./TripScore";
import { Event, useEvents } from "@/hooks/EventsApiHook";
import { ScoreModifiedRelative } from "@/types/events/Scores";
import { useHotkeys } from "react-hotkeys-hook";
import { cn } from "@/lib/utils";
import { JamScore } from "./JamScore";

type TeamControlsProps = {
    gameId?: string;
    side: TeamSide;
}

export const TeamControls = ({ gameId, side }: TeamControlsProps) => {

    const { sendEvent } = useEvents();

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
        sendEventIfIdSet(new ScoreModifiedRelative({ teamSide: side, value: 1 }));
    }

    const decrementScore = () => {
        sendEventIfIdSet(new ScoreModifiedRelative({ teamSide: side, value: -1 }));
    }

    const setTripScore = (score: number) => {
        const scoreDelta = score - (tripScore?.score ?? 0);
        sendEventIfIdSet(new ScoreModifiedRelative({ teamSide: side, value: scoreDelta }));
    }

    const tripShortcutKeys: string[] = [];
    tripShortcutKeys[4] = side === TeamSide.Home ? "🠅s" : "🠅#";

    useHotkeys(side === TeamSide.Home ? 'a' : '\'', decrementScore, { preventDefault: true });
    useHotkeys(side === TeamSide.Home ? 's' : '#', incrementScore, { preventDefault: true });
    useHotkeys(side === TeamSide.Home ? 'shift+s' : 'shift+#', () => setTripScore(4), { preventDefault: true });

    return (
        <Card className={cn("grow inline-block mt-5", side === TeamSide.Home ? "mr-2.5" : "ml-2.5")}>
            <CardHeader className="p-2">
                <CardTitle className="text-center text-xl">{teamName}</CardTitle>
            </CardHeader>
            <CardContent className="py-0">
                <Separator />
                <JamScore side={side} />
                <div className="flex w-full justify-center items-center">
                    <Button onClick={decrementScore} variant="secondary" className="text-md lg:text-xl">-1 [{side === TeamSide.Home ? 'a' : '\''}]</Button>
                    <TeamScore side={side} />
                    <Button onClick={incrementScore} variant="secondary" className="text-md lg:text-xl" >+1 [{side === TeamSide.Home ? 's' : '#'}]</Button>
                </div>
                <Separator />
                <TripScore tripScore={tripScore?.score ?? 0} scoreShortcutKeys={tripShortcutKeys} onTripScoreSet={setTripScore} />
            </CardContent>
        </Card>
    )
}