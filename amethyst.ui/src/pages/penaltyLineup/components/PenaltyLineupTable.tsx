import { Button } from "@/components/ui";
import { useEvents, useGameStageState, useI18n, useLineupSheetState, usePenaltyBoxState, usePenaltySheetState, useTeamDetailsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { LineupPosition, Penalty, StringMap, TeamSide } from "@/types";
import { useEffect, useMemo, useState } from "react";
import { PenaltyDialog } from "./PenaltyDialog";
import { PenaltyAssessed, SkaterAddedToJam, SkaterInjuryAdded, SkaterPosition, SkaterReleasedFromBox, SkaterRemovedFromJam, SkaterSatInBox } from "@/types/events";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { PenaltyLineupRow } from "./PenaltyLineupRow";

type PenaltyLineupTableProps = {
    teamSide: TeamSide;
    gameId: string;
    compact?: boolean;
}

export const PenaltyLineupTable = ({ teamSide, gameId, compact }: PenaltyLineupTableProps) => {
    const { translate } = useI18n({ prefix: "PenaltyLineup.PenaltyLineupTable."});
    const { team } = useTeamDetailsState(teamSide) ?? {};
    const { jams } = useLineupSheetState(teamSide) ?? { jams: [] };
    const penaltyBox = usePenaltyBoxState(teamSide) ?? { skaters: [] };
    const gameStage = useGameStageState();
    const penaltySheet = usePenaltySheetState(teamSide) ?? { lines: [] };
    const { sendEvent } = useEvents();
    const [penaltyDialogOpen, setPenaltyDialogOpen] = useState(false);
    const [editingSkaterNumber, setEditingSkaterNumber] = useState("");
    const [editingIndex, setEditingIndex] = useState(0);
    const [editingPenalty, setEditingPenalty] = useState<Penalty>();
    const [totalJamNumber, setTotalJamNumber] = useState(-1);

    const skaterPenalties = useMemo(() => 
        penaltySheet.lines.reduce((map, line) => ({ ...map, [line.skaterNumber]: line.penalties }), {} as StringMap<Penalty[]>), 
    [penaltySheet]);

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

    if(!team || !gameStage) {
        return (<></>);
    }

    const headerClass = "border-b-2 border-black";
    const headerTextClass = "font-bold flex text-center justify-center items-end";

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

    const handleBoxClicked = (skaterNumber: string, inBox: boolean) => {
        if(inBox) {
            sendEvent(gameId, new SkaterSatInBox(teamSide, skaterNumber));
        } else {
            sendEvent(gameId, new SkaterReleasedFromBox(teamSide, skaterNumber));
        }
    }

    const handlePenaltyClicked = (skaterNumber: string, index: number) => {
        const selectedSkaterPenalties = skaterPenalties[skaterNumber]!;
        setEditingSkaterNumber(skaterNumber);
        setEditingIndex(index);
        if(index < selectedSkaterPenalties.length) {
            setEditingPenalty(selectedSkaterPenalties[index]);
        } else {
            setEditingPenalty(undefined);
        }
        setPenaltyDialogOpen(true);
    }

    const handlePenaltyAccept = (penalty: Penalty) => {
        const selectedSkaterPenalties = [...skaterPenalties[editingSkaterNumber]!];
        if(editingIndex < selectedSkaterPenalties.length) {
            selectedSkaterPenalties[editingIndex] = penalty;
        } else {
            selectedSkaterPenalties.push(penalty);
        }
        
        sendEvent(gameId, new PenaltyAssessed(teamSide, editingSkaterNumber, penalty.code));
    }

    const handleInjuryAdded = (skaterNumber: string) => {
        sendEvent(gameId, new SkaterInjuryAdded(teamSide, skaterNumber));
    }

    return (
        <>
            <div className={cn(
                "w-full grid grid-flow-row grid-cols-[auto_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr]",
            )}
            >
                <div className={cn("col-start-2 flex items-end", headerClass)}>
                    <Button 
                        className="w-full p-0 lg:p-2"
                        variant="secondary" 
                        disabled={totalJamNumber <= 0} 
                        onClick={() => setTotalJamNumber(i => i - 1)}
                    >
                        <ChevronLeft />
                        <span className={cn("hidden", !compact && "lg:inline")}>{translate("PreviousJam")}</span>
                    </Button>
                </div>
                <div className={cn("col-start-3 col-span-3 gap-2", headerClass, headerTextClass, "items-center md:items-end")}>
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
                <div className={cn("col-start-6 flex items-end", headerClass)}>
                    <Button 
                        className="w-full p-0 lg:p-2"
                        disabled={totalJamNumber >= jams.length - 1 || totalJamNumber === -1} 
                        onClick={() => setTotalJamNumber(i => i + 1)}
                    >
                        <span className={cn("hidden", !compact && "lg:inline")}>{translate("NextJam")}</span>
                        <ChevronRight />
                    </Button>
                </div>
                <div className={cn("col-start-7 col-span-10", headerClass)}></div>
                <div className={cn("col-start-17", headerClass, headerTextClass)}>
                    <span className={cn("hidden sm:inline", !compact && "2xl:hidden", "text-xs lg:text-base")}>{translate("FouloutExpulsion.Short")}</span>
                    <span className={cn("hidden", !compact && "2xl:inline")}>{translate("FouloutExpulsion.Long")}</span>
                </div>
                <div className={cn("col-start-18", headerClass, headerTextClass)}>
                    <span className={cn("hidden sm:inline", !compact && "2xl:hidden", "text-xs lg:text-base")}>{translate("TotalPenalties.Short")}</span>
                    <span className={cn("hidden", !compact && "2xl:inline")}>{translate("TotalPenalties.Long")}</span>
                </div>
                { team.roster.filter(s => s.isSkating).map((s, i) => (
                    <PenaltyLineupRow 
                        key={s.number}
                        even={i % 2 == 0} 
                        skater={s} 
                        position={skaterPositions?.[s.number] ?? LineupPosition.Bench}
                        penalties={skaterPenalties[s.number] ?? []}
                        inBox={penaltyBox.skaters.includes(s.number)}
                        injured={i % 2 == 0}
                        compact={compact}
                        disableBox={(currentJam?.jam ?? -1) < gameStage.jamNumber || currentJam?.period !== gameStage.periodNumber}
                        onPositionClicked={(position: LineupPosition) => handlePositionClicked(s.number, position)}
                        onBoxClicked={inBox => handleBoxClicked(s.number, inBox)}
                        onPenaltyClicked={(index: number) => handlePenaltyClicked(s.number, index)} 
                        onInjuryAdded={() => handleInjuryAdded(s.number)}
                    />
                ))}
                <div className="col-start-2 col-span-12 border-t border-black"></div>
                <div className="col-start-14 col-span-5 border-t border-black"></div>
            </div>
            <PenaltyDialog open={penaltyDialogOpen} currentPenalty={editingPenalty} onOpenChanged={setPenaltyDialogOpen} onAccept={handlePenaltyAccept} />
        </>
    )
}