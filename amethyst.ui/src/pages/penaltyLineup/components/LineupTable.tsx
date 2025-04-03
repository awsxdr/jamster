import { Button } from "@/components/ui";
import { useEvents, useGameStageState, useI18n, useInjuriesState, useLineupSheetState, usePenaltySheetState, useRulesState, useTeamDetailsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { LineupPosition, Penalty, StringMap, TeamSide } from "@/types";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { MenuColumn, SkaterNumberColumn } from ".";
import { useEffect, useMemo, useState } from "react";
import { SkaterPositionColumn } from "./SkaterPositionColumn";
import { SkaterAddedToJam, SkaterInjuryAdded, SkaterInjuryRemoved, SkaterPosition, SkaterReleasedFromBox, SkaterRemovedFromJam, SkaterSatInBox } from "@/types/events";
import { PenaltyBoxColumn } from "./PenaltyBoxColumn";

type LineupTableProps = {
    gameId: string;
    teamSide: TeamSide,
    compact?: boolean;
}

export const LineupTable = ({
        gameId,
        teamSide,
        compact, 
    }: LineupTableProps) => {

    const { translate } = useI18n({ prefix: "PenaltyLineup.LineupTable." })

    const { jams } = useLineupSheetState(teamSide) ?? { jams: [] };
    const { team } = useTeamDetailsState(teamSide) ?? {};
    const { injuries } = useInjuriesState(teamSide) ?? { injuries: [] };
    const penaltySheet = usePenaltySheetState(teamSide) ?? { lines: [] };
    const { rules } = useRulesState() ?? { };    
    const { sendEvent } = useEvents();

    const gameStage = useGameStageState();

    const [totalJamNumber, setTotalJamNumber] = useState(-1);

    useEffect(() => {
        if(totalJamNumber > -1 || !gameStage) {
            return;
        }

        setTotalJamNumber(jams.findIndex(j => j.jam === gameStage.jamNumber + 1 && j.period === gameStage.periodNumber));

    }, [jams, gameStage])

    const currentJam = useMemo(() => {
        if(totalJamNumber < 0 || totalJamNumber > jams.length) {
            return undefined;
        }

        return jams[totalJamNumber];
    }, [totalJamNumber, jams]);

    const skaterNumbers = team?.roster.filter(s => s.isSkating).map(s => s.number).sort() ?? [];

    const skaterPositions = useMemo(() =>
        team?.roster.reduce((map, { number }) => ({
            ...map,
            [number]: (
                currentJam?.jammerNumber === number ? LineupPosition.Jammer
                : currentJam?.pivotNumber === number ? LineupPosition.Pivot
                : currentJam?.blockerNumbers.includes(number) ? LineupPosition.Blocker
                : LineupPosition.Bench
            )
        }), {} as StringMap<LineupPosition>),
        [currentJam, team]);

    const skaterPenalties = useMemo(() => 
        penaltySheet.lines.reduce((map, line) => ({ ...map, [line.skaterNumber]: line.penalties }), {} as StringMap<Penalty[]>), 
    [penaltySheet]);

    const skaterExpulsions = useMemo(() =>
        penaltySheet.lines.reduce((map, line) => ({ ...map, [line.skaterNumber]: line.expulsionPenalty }), {} as StringMap<Penalty | null>),
    [penaltySheet]);

    const offTrackSkaters = useMemo(() => 
        Object.keys(skaterPenalties).filter(n => 
            (skaterPenalties[n]?.length ?? 0) >= (rules?.penaltyRules.foulOutPenaltyCount ?? 0)
            || skaterExpulsions[n] !== null
        ),
    [skaterPenalties, skaterExpulsions, injuries, rules]);

    const injuredSkaters = useMemo(() =>
        injuries.filter(i => !i.expired).map(i => i.skaterNumber), [injuries]);

    const handlePositionClicked = (skaterNumber: string, position: LineupPosition) => {
        if(!currentJam) {
            return;
        }

        if(position === LineupPosition.Bench) {
            sendEvent(gameId, new SkaterRemovedFromJam(
                teamSide, 
                currentJam.period,
                currentJam.jam,
                skaterNumber
            ));
        } else {
            sendEvent(gameId, new SkaterAddedToJam(
                teamSide,
                currentJam.period,
                currentJam.jam,
                position as unknown as SkaterPosition, 
                skaterNumber));
        }
    }

    const handleBoxClicked = (skaterNumber: string, currentlyInBox: boolean) => {
        if(currentlyInBox) {
            sendEvent(gameId, new SkaterReleasedFromBox(teamSide, skaterNumber));
        } else {
            sendEvent(gameId, new SkaterSatInBox(teamSide, skaterNumber));
        }
    }

    if(!skaterPositions) {
        return (<></>);
    }

    const headerClassName = "border-b-2 border-black";
    const headerTextClassName = "font-bold flex text-center justify-center items-end";

    const handleInjuryAdded = (skaterNumber: string) => {
        sendEvent(gameId, new SkaterInjuryAdded(teamSide, skaterNumber));
    }

    const handleInjuryRemoved = (skaterNumber: string) => {
        const injuryJamNumber = injuries.filter(i => i.skaterNumber === skaterNumber).at(-1)?.totalJamNumberStart;

        if(injuryJamNumber === undefined) {
            return;
        }

        sendEvent(gameId, new SkaterInjuryRemoved(teamSide, skaterNumber, injuryJamNumber));
    }

    return (
        <>
            <div className={cn("col-start-2 row-start-1 flex items-end", headerClassName)}>
                <Button 
                    className="w-full p-0 lg:p-2 rounded-none"
                    variant="secondary" 
                    disabled={totalJamNumber <= 0} 
                    onClick={() => setTotalJamNumber(t => t - 1)}
                >
                    <ChevronLeft />
                    <span className={cn("hidden", !compact && "lg:inline")}>{translate("PreviousJam")}</span>
                </Button>
            </div>
            <div className={cn("col-start-3 col-span-4 row-start-1 gap-2", headerClassName, headerTextClassName, "items-center")}>
                <span>
                    <span className={cn(!compact && "lg:hidden")}>{translate("Period.Short")}</span>
                    <span className={cn("hidden", !compact && "lg:inline")}>{translate("Period.Long")}</span>
                    { currentJam?.period }
                </span>
                <span>-</span>
                <span>
                    <span className={cn(!compact && "lg:hidden")}>{translate("Jam.Short")}</span>
                    <span className={cn("hidden", !compact && "lg:inline")}>{translate("Jam.Long")}</span>
                    { currentJam?.jam }
                </span>
            </div>
            <div className={cn("col-start-7 row-start-1 flex items-end", headerClassName)}>
                <Button 
                    className="w-full p-0 lg:p-2 rounded-none"
                    disabled={totalJamNumber >= jams.length - 1 || totalJamNumber === -1} 
                    onClick={() => setTotalJamNumber(t => t + 1)}
                >
                    <span className={cn("hidden", !compact && "lg:inline")}>{translate("NextJam")}</span>
                    <ChevronRight />
                </Button>
            </div>
            <MenuColumn 
                skaterNumbers={skaterNumbers} 
                skaterPositions={skaterPositions}
                injuredSkaters={injuredSkaters}
                onInjuryAdded={handleInjuryAdded}
                onInjuryRemoved={handleInjuryRemoved} 
            />
            <SkaterNumberColumn 
                skaterNumbers={skaterNumbers}
                skaterPositions={skaterPositions} 
                skaterPenalties={skaterPenalties} 
                offTrackSkaters={offTrackSkaters} 
                injuredSkaters={injuredSkaters} 
                compact={compact}
            />
            <SkaterPositionColumn
                position={LineupPosition.Bench}
                skaterNumbers={skaterNumbers}
                selectedSkaters={skaterNumbers.filter(s => currentJam?.jammerNumber !== s && currentJam?.pivotNumber !== s && !currentJam?.blockerNumbers.includes(s))}
                offTrackSkaters={offTrackSkaters}
                injuredSkaters={injuredSkaters} 
                compact={compact}
                className="col-start-3"
                onSkaterClicked={s => handlePositionClicked(s, LineupPosition.Bench)}
            />
            <SkaterPositionColumn
                position={LineupPosition.Jammer}
                skaterNumbers={skaterNumbers}
                selectedSkaters={currentJam?.jammerNumber ? [currentJam?.jammerNumber] : []}
                offTrackSkaters={offTrackSkaters}
                injuredSkaters={injuredSkaters} 
                compact={compact}
                className="col-start-4"
                onSkaterClicked={s => handlePositionClicked(s, LineupPosition.Jammer)}
            />
            <SkaterPositionColumn
                position={LineupPosition.Pivot}
                skaterNumbers={skaterNumbers}
                selectedSkaters={currentJam?.pivotNumber ? [currentJam?.pivotNumber] : []}
                offTrackSkaters={offTrackSkaters}
                injuredSkaters={injuredSkaters} 
                compact={compact}
                className="col-start-5"
                onSkaterClicked={s => handlePositionClicked(s, LineupPosition.Pivot)}
            />
            <SkaterPositionColumn
                position={LineupPosition.Blocker}
                skaterNumbers={skaterNumbers}
                selectedSkaters={currentJam?.blockerNumbers.filter(b => b) as string[] ?? []}
                offTrackSkaters={offTrackSkaters}
                injuredSkaters={injuredSkaters} 
                compact={compact}
                className="col-start-6 border-r-2"
                onSkaterClicked={s => handlePositionClicked(s, LineupPosition.Blocker)}
            />
            <PenaltyBoxColumn
                teamSide={teamSide}
                skaterNumbers={skaterNumbers}
                skaterPenalties={skaterPenalties}
                compact={compact}
                onClick={handleBoxClicked}
            />
            <div className="col-start-2 col-span-6 border-t border-black">
            </div>
        </>
    );
}