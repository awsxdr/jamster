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
    disabled?: boolean;
}

export const TeamControls = ({ gameId, side, disabled }: TeamControlsProps) => {

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
                    <Button onClick={decrementScore} variant="secondary" disabled={disabled} className="text-md lg:text-xl">-1 [{side === TeamSide.Home ? 'a' : '\''}]</Button>
                    <TeamScore side={side} />
                    <Button onClick={incrementScore} variant="secondary" disabled={disabled} className="text-md lg:text-xl" >+1 [{side === TeamSide.Home ? 's' : '#'}]</Button>
                </div>
                <Separator />
                <TripScore tripScore={tripScore?.score ?? 0} disabled={disabled} scoreShortcutKeys={tripShortcutKeys} onTripScoreSet={setTripScore} />
                <Separator />
                <div className="flex w-full justify-center items-center gap-2 p-5">
                    <Button variant="default">Initial trip [{side === TeamSide.Home ? "d" : ";"}]</Button>
                    <Button variant="secondary">Lost [{side === TeamSide.Home ? "🠅d" : "🠅;"}]</Button>
                    <Button variant="secondary">Star pass [{side === TeamSide.Home ? "x" : "/"}]</Button>
                </div>
                <Separator />
                <div className="flex justify-center items-center self-center">
                    <div className="flex flex-col items-end">
                        <div className="flex flex-wrap gap-2 pt-5 pb-1 items-baseline">
                            <span>Jammer</span>
                            <span className="flex gap-0.5">
                                <Button size="sm" variant="secondary" className="border-2 border-lime-600">?</Button>
                                <Button size="sm" variant="secondary" className="border-2">0</Button>
                                <Button size="sm" variant="secondary" className="border-2">01</Button>
                                <Button size="sm" variant="secondary" className="border-2">123</Button>
                                <Button size="sm" variant="secondary" className="border-2">17</Button>
                                <Button size="sm" variant="secondary" className="border-2">29</Button>
                                <Button size="sm" variant="secondary" className="border-2">404</Button>
                                <Button size="sm" variant="secondary" className="border-2">49</Button>
                                <Button size="sm" variant="secondary" className="border-2">52</Button>
                                <Button size="sm" variant="secondary" className="border-2">77</Button>
                                <Button size="sm" variant="secondary" className="border-2">89</Button>
                            </span>
                        </div>
                        <div className="flex flex-wrap justify-center items-center gap-2 pb-5 items-baseline">
                            <span>Pivot</span>
                            <span className="flex gap-0.5">
                                <Button size="sm" variant="secondary" className="border-2 border-lime-600">?</Button>
                                <Button size="sm" variant="secondary" className="border-2">0</Button>
                                <Button size="sm" variant="secondary" className="border-2">01</Button>
                                <Button size="sm" variant="secondary" className="border-2">123</Button>
                                <Button size="sm" variant="secondary" className="border-2">17</Button>
                                <Button size="sm" variant="secondary" className="border-2">29</Button>
                                <Button size="sm" variant="secondary" className="border-2">404</Button>
                                <Button size="sm" variant="secondary" className="border-2">49</Button>
                                <Button size="sm" variant="secondary" className="border-2">52</Button>
                                <Button size="sm" variant="secondary" className="border-2">77</Button>
                                <Button size="sm" variant="secondary" className="border-2">89</Button>
                            </span>
                        </div>
                    </div>
                </div>
            </CardContent>
        </Card>
    )
}