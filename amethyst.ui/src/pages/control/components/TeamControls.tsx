import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useJamLineupState, useTeamDetailsState, useTripScoreState } from "@/hooks";
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
import { RadioButtonGroup } from "@/components/RadioButtonGroup";
import { SkaterOnTrack, SkaterPosition } from "@/types/events/JamLineup";
import { useUserSettings } from "@/hooks/UserSettings";

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

    const tripShortcutKeys: string[] = [];
    tripShortcutKeys[4] = side === TeamSide.Home ? "🠅s" : "🠅#";

    useHotkeys(side === TeamSide.Home ? 'a' : '\'', decrementScore, { preventDefault: true });
    useHotkeys(side === TeamSide.Home ? 's' : '#', incrementScore, { preventDefault: true });
    useHotkeys(side === TeamSide.Home ? 'shift+s' : 'shift+#', () => setTripScore(4), { preventDefault: true });

    const skaterNumbers = ["0", "01", "123", "17", "29", "404", "49", "52", "77", "89" ];

    const lineup = useJamLineupState(side);
    const handleLineupSelected = (position: SkaterPosition, number: string | null) =>
        sendEventIfIdSet(new SkaterOnTrack(side, position, number));

    return (
        <Card className={cn("grow inline-block mt-5", side === TeamSide.Home ? "mr-2.5" : "ml-2.5")}>
            <CardHeader className="p-2">
                <CardTitle className="text-center text-xl">{teamName}</CardTitle>
            </CardHeader>
            <CardContent className="py-0">
                <Separator />
                { userSettings.showScoreControls && (
                    <>
                        <JamScore side={side} />
                        <div className="flex w-full justify-center items-center">
                            <Button onClick={decrementScore} variant="secondary" disabled={disabled} className="text-md lg:text-xl">-1 [{side === TeamSide.Home ? 'a' : '\''}]</Button>
                            <TeamScore side={side} />
                            <Button onClick={incrementScore} variant="secondary" disabled={disabled} className="text-md lg:text-xl" >+1 [{side === TeamSide.Home ? 's' : '#'}]</Button>
                        </div>
                        <Separator />
                        <TripScore tripScore={tripScore?.score ?? 0} disabled={disabled} scoreShortcutKeys={tripShortcutKeys} onTripScoreSet={setTripScore} />
                        <Separator />
                    </>
                )}
                { userSettings.showStatsControls && (
                    <>
                        <div className="flex w-full justify-center items-center gap-2 p-5">
                            <Button variant="default">Initial trip [{side === TeamSide.Home ? "d" : ";"}]</Button>
                            <Button variant="secondary">Lost [{side === TeamSide.Home ? "🠅d" : "🠅;"}]</Button>
                            <Button variant="secondary">Star pass [{side === TeamSide.Home ? "x" : "/"}]</Button>
                        </div>
                        <Separator />
                    </>
                )}
                { userSettings.showLineupControls && (
                    <>
                        <div className="flex justify-center items-center self-center">
                            <div className="flex flex-col items-end">
                                <div className="flex flex-wrap justify-center items-center gap-2 pt-5 pb-1 items-baseline">
                                    <span>Jammer</span>
                                    <RadioButtonGroup
                                        items={[{value: null, name: "?"}, ...skaterNumbers.map(s => ({ value: s, name: s}))]}
                                        value={lineup?.jammerNumber}
                                        rowClassName="gap-0.5"
                                        size="sm"
                                        onItemSelected={v => handleLineupSelected(SkaterPosition.Jammer, v)}
                                    />
                                </div>
                                <div className="flex flex-wrap justify-center items-center gap-2 pb-5 items-baseline">
                                    <span>Pivot</span>
                                    <RadioButtonGroup
                                        items={[{value: null, name: "?"}, ...skaterNumbers.map(s => ({ value: s, name: s}))]}
                                        value={lineup?.pivotNumber}
                                        rowClassName="gap-0.5"
                                        size="sm"
                                        onItemSelected={v => handleLineupSelected(SkaterPosition.Pivot, v)}
                                    />
                                </div>
                            </div>
                        </div>
                    </>
                )}
            </CardContent>
        </Card>
    )
}