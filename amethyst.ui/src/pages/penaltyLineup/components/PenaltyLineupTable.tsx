import { Button } from "@/components/ui";
import { useEvents, useGameStageState, useI18n, useInjuriesState, useLineupSheetState, usePenaltyBoxState, usePenaltySheetState, useRulesState, useTeamDetailsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { LineupPosition, Penalty, StringMap, TeamSide } from "@/types";
import { CSSProperties, useEffect, useMemo, useState } from "react";
import { PenaltyDialog } from "./PenaltyDialog";
import { ExpulsionCleared, PenaltyAssessed, PenaltyRescinded, PenaltyUpdated, SkaterAddedToJam, SkaterExpelled, SkaterInjuryAdded, SkaterPosition, SkaterReleasedFromBox, SkaterRemovedFromJam, SkaterSatInBox } from "@/types/events";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { PenaltyLineupRow } from "./PenaltyLineupRow";
import { ExpulsionDialog } from "./ExpulsionDialog";

type PenaltyLineupDisplay = "Penalties" | "Lineup" | "Both";

type PenaltyLineupTableProps = {
    teamSide: TeamSide;
    gameId: string;
    compact?: boolean;
    display: PenaltyLineupDisplay;
}

export const PenaltyLineupTable = ({ teamSide, gameId, compact, display }: PenaltyLineupTableProps) => {
    const { translate } = useI18n({ prefix: "PenaltyLineup.PenaltyLineupTable."});
    const { team } = useTeamDetailsState(teamSide) ?? {};
    const { jams } = useLineupSheetState(teamSide) ?? { jams: [] };
    const penaltyBox = usePenaltyBoxState(teamSide) ?? { skaters: [] };
    const gameStage = useGameStageState();
    const penaltySheet = usePenaltySheetState(teamSide) ?? { lines: [] };
    const { sendEvent } = useEvents();
    const [penaltyDialogOpen, setPenaltyDialogOpen] = useState(false);
    const [expulsionDialogOpen, setExpulsionDialogOpen] = useState(false);
    const [editingSkaterNumber, setEditingSkaterNumber] = useState("");
    const [editingIndex, setEditingIndex] = useState(0);
    const [editingPenalty, setEditingPenalty] = useState<Penalty>();
    const [totalJamNumber, setTotalJamNumber] = useState(-1);
    const { injuries } = useInjuriesState(teamSide) ?? { injuries: [] };
    const { rules } = useRulesState() ?? { };

    const skaterPenalties = useMemo(() => 
        penaltySheet.lines.reduce((map, line) => ({ ...map, [line.skaterNumber]: line.penalties }), {} as StringMap<Penalty[]>), 
    [penaltySheet]);

    const skaterExpulsions = useMemo(() =>
        penaltySheet.lines.reduce((map, line) => ({ ...map, [line.skaterNumber]: line.expulsionPenalty }), {} as StringMap<Penalty | null>),
    [penaltySheet]);

    const teamName = useMemo(() => {
        if(!team) {
            return '';
        }

        return team.names['controls'] || team.names['color'] ||  team.names['team'] || team.names['league'] || '';
    }, [team]);

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

    if(!team || !gameStage || !rules) {
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
            const originalPenalty = selectedSkaterPenalties[editingIndex];
            sendEvent(gameId, new PenaltyUpdated(
                teamSide, 
                editingSkaterNumber, 
                originalPenalty.code, 
                originalPenalty.period, 
                originalPenalty.jam,
                penalty.code,
                penalty.period,
                penalty.jam
            ));
        } else {
            sendEvent(gameId, new PenaltyAssessed(teamSide, editingSkaterNumber, penalty.code));
        }
    }

    const handlePenaltyDelete = () => {
        const selectedSkaterPenalties = [...skaterPenalties[editingSkaterNumber]!];
        if(editingIndex >= selectedSkaterPenalties.length) {
            return;
        }

        const penalty = selectedSkaterPenalties[editingIndex];
        sendEvent(gameId, new PenaltyRescinded(
            teamSide, 
            editingSkaterNumber, 
            penalty.code, 
            penalty.period, 
            penalty.jam));
    }

    const handleExpulsionClicked = (skaterNumber: string) => {
        setEditingSkaterNumber(skaterNumber);
        setExpulsionDialogOpen(true);
    }

    const handleExpulsionAccept = (penalty: Penalty | null) => {
        if(penalty === null) {
            sendEvent(gameId, new ExpulsionCleared(teamSide, editingSkaterNumber));
        } else {
            sendEvent(gameId, new SkaterExpelled(teamSide, editingSkaterNumber, penalty.code, penalty.period, penalty.jam));
        }
    }

    const handleInjuryAdded = (skaterNumber: string) => {
        sendEvent(gameId, new SkaterInjuryAdded(teamSide, skaterNumber));
    }

    const columnCount =
        display === "Both" ? 17
        : display === "Lineup" ? 6
        : display === "Penalties" ? 12
        : 0;

    const columns = `auto repeat(${columnCount}, 1fr)`

    return (
        <>
            <div className="w-full text-center font-bold">{teamName}</div>
            <div 
                className={cn(
                    "w-full grid grid-flow-row grid-cols-[--plt-columns]",
                )}
                style={{ "--plt-columns": columns } as CSSProperties}
            >
                { display !== "Penalties" && (
                    <>
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
                        <div className={cn("col-start-3 col-span-4 gap-2", headerClass, headerTextClass, "items-center md:items-end")}>
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
                        <div className={cn("col-start-7 flex items-end", headerClass)}>
                            <Button 
                                className="w-full p-0 lg:p-2"
                                disabled={totalJamNumber >= jams.length - 1 || totalJamNumber === -1} 
                                onClick={() => setTotalJamNumber(i => i + 1)}
                            >
                                <span className={cn("hidden", !compact && "lg:inline")}>{translate("NextJam")}</span>
                                <ChevronRight />
                            </Button>
                        </div>
                        </>
                )}
                <div className={cn(display === "Penalties" ? "col-start-2 col-span-10" : "col-start-8 col-span-9", headerClass)}></div>
                { display !== "Lineup" && (
                    <>
                        <div className={cn(display === "Both" ? "col-start-17" : "col-start-12", headerClass, headerTextClass)}>
                            <span className={cn("hidden sm:inline", !compact && "2xl:hidden", "text-xs lg:text-base")}>{translate("FouloutExpulsion.Short")}</span>
                            <span className={cn("hidden", !compact && "2xl:inline")}>{translate("FouloutExpulsion.Long")}</span>
                        </div>
                        <div className={cn(display === "Both" ? "col-start-17" : "col-start-13", headerClass, headerTextClass)}>
                            <span className={cn("hidden sm:inline", !compact && "2xl:hidden", "text-xs lg:text-base")}>{translate("TotalPenalties.Short")}</span>
                            <span className={cn("hidden", !compact && "2xl:inline")}>{translate("TotalPenalties.Long")}</span>
                        </div>
                    </>
                )}
                { team.roster.filter(s => s.isSkating).map((s, i) => (
                    <PenaltyLineupRow 
                        key={s.number}
                        even={i % 2 == 0} 
                        skater={s} 
                        position={skaterPositions?.[s.number] ?? LineupPosition.Bench}
                        expulsionPenalty={skaterExpulsions[s.number] ?? null}
                        penalties={skaterPenalties[s.number] ?? []}
                        rules={rules}
                        inBox={penaltyBox.skaters.includes(s.number)}
                        injured={injuries.some(i => !i.expired && i.skaterNumber === s.number)}
                        compact={compact}
                        display={display}
                        disableBox={(currentJam?.jam ?? -1) < gameStage.jamNumber || currentJam?.period !== gameStage.periodNumber}
                        onPositionClicked={(position: LineupPosition) => handlePositionClicked(s.number, position)}
                        onBoxClicked={inBox => handleBoxClicked(s.number, inBox)}
                        onPenaltyClicked={(index: number) => handlePenaltyClicked(s.number, index)}
                        onExpulsionClicked={() => handleExpulsionClicked(s.number)}
                        onInjuryAdded={() => handleInjuryAdded(s.number)}
                    />
                ))}
                { display !== "Penalties" && (
                    <div className="col-start-2 col-span-6 border-t border-black"></div> 
                )}
                { display !== "Lineup" && (
                    <div className={cn("col-span-12 border-t border-black", display === "Both" ? "col-start-8" : "col-start-2")}></div>
                )}
            </div>
            <PenaltyDialog 
                open={penaltyDialogOpen} 
                currentPenalty={editingPenalty} 
                onOpenChanged={setPenaltyDialogOpen} 
                onAccept={handlePenaltyAccept} 
                onDelete={handlePenaltyDelete} 
            />
            <ExpulsionDialog
                open={expulsionDialogOpen}
                expulsionPenalty={skaterExpulsions[editingSkaterNumber] ?? null}
                penalties={skaterPenalties[editingSkaterNumber] ?? []}
                onOpenChanged={setExpulsionDialogOpen}
                onAccept={handleExpulsionAccept}
            />
        </>
    )
}