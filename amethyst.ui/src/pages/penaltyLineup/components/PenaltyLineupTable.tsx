import { Button } from "@/components/ui";
import { useEvents, useGameStageState, useLineupSheetState, useTeamDetailsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { GameSkater, StringMap, TeamSide } from "@/types";
import { useEffect, useMemo, useState } from "react";
import { PenaltyDialog, RecordedPenalty } from "./PenaltyDialog";
import { SkaterAddedToJam, SkaterPosition, SkaterRemovedFromJam } from "@/types/events";
import { ChevronLeft, ChevronRight } from "lucide-react";

enum Position {
    Bench = "Bench",
    Jammer = "Jammer",
    Pivot = "Pivot",
    Blocker = "Blocker",
}

type PenaltyCellProps = {
    penalty?: RecordedPenalty;
    className?: string;
    onClick?: () => void;
}

const PenaltyCell = ({penalty, className, onClick}: PenaltyCellProps) => {
    return (
        <div 
            className={cn(
                className,
                "flex flex-col xl:flex-row xl:gap-2 text-xs xl:text-sm font-bold text-center justify-center align-center items-center", 
            )} 
            onClick={onClick}
        >
            {penalty && (
                <>
                    <div>{penalty.penaltyCode}</div>
                    <div>{penalty.period}-{penalty.jam}</div>
                </>
            )}
        </div>
    );
}

type PositionButtonProps = {
    position: Position;
    targetPosition: Position;
    text: string;
    shortText: string;
    className?: string;
    rowClassName?: string;
    onClick?: (position: Position) => void;
}

const PositionButton = ({ position, targetPosition, text, shortText, className, rowClassName, onClick }: PositionButtonProps) => (
    <Button 
        className={cn(className, position !== targetPosition && rowClassName)}
        variant={position === targetPosition ? "default" : "outline" }
        onClick={() => onClick?.(targetPosition)}
    >
        <span className="lg:hidden">{shortText}</span>
        <span className="hidden lg:inline">{text}</span>
    </Button>
)

type PenaltyLineupRowProps = {
    skater: GameSkater;
    position: Position;
    even: boolean;
    penalties: RecordedPenalty[];
    disableBox?: boolean;
    onPositionClicked?: (position: Position) => void;
    onPenaltyClicked?: (index: number) => void;
}

const PenaltyLineupRow = ({ skater, position, even, penalties, disableBox, onPositionClicked, onPenaltyClicked }: PenaltyLineupRowProps) => {

    const cellClass = "border-b border-r border-black";

    const lineupRowClass = even
        ? "bg-white dark:bg-gray-900"
        : "bg-blue-100 dark:bg-sky-950";

    const lineupRowClassAccent = even
        ? "border-black bg-blue-100 dark:bg-sky-950"
        : "border-black bg-blue-200 dark:bg-sky-800";

    const penaltyRowClass = even
        ? "bg-white dark:bg-rose-950"
        : "bg-red-100 dark:bg-rose-900";

    const penaltyRowClassAccent = even
        ? "bg-red-100 dark:bg-rose-900"
        : "bg-red-200 dark:bg-rose-700";

    const penaltyRowClassFullAccent = "bg-red-200 dark:bg-rose-700";

    const largeNumberClass = "flex justify-center items-center text-sm sm:text-base md:text-lg";

    const buttonClass = "rounded-none w-full px-1 md:px-4 border-0";

    const handlePenaltyClicked = (index: number) => {
        onPenaltyClicked?.(index);
    }

    return (
        <>
            <div className={cn("col-start-1", "border-l-2", cellClass, largeNumberClass, lineupRowClassAccent)}>
                {skater.number}
            </div>
            <div className={cn("col-start-2", "border-l", cellClass, lineupRowClass)}>
                <PositionButton 
                    position={position} 
                    targetPosition={Position.Bench} 
                    text="Bench" 
                    shortText="Bch" 
                    className={buttonClass} 
                    rowClassName={lineupRowClass}
                    onClick={onPositionClicked}
                />
            </div>
            <div className={cn("col-start-3", cellClass, lineupRowClass)}>
                <PositionButton 
                    position={position} 
                    targetPosition={Position.Jammer} 
                    text="Jammer" 
                    shortText="J" 
                    className={buttonClass} 
                    rowClassName={lineupRowClass}
                    onClick={onPositionClicked}
                />
            </div>
            <div className={cn("col-start-4", cellClass, lineupRowClass)}>
                <PositionButton 
                    position={position} 
                    targetPosition={Position.Pivot} 
                    text="Pivot" 
                    shortText="P" 
                    className={buttonClass} 
                    rowClassName={lineupRowClass}
                    onClick={onPositionClicked}
                />
            </div>
            <div className={cn("col-start-5", cellClass, lineupRowClass)}>
                <PositionButton 
                    position={position} 
                    targetPosition={Position.Blocker} 
                    text="Blocker" 
                    shortText="B" 
                    className={buttonClass} 
                    rowClassName={lineupRowClass}
                    onClick={onPositionClicked}
                />
            </div>
            <div className={cn("col-start-6", cellClass, lineupRowClassAccent)}>
                <Button 
                    className={cn(buttonClass, lineupRowClassAccent)}
                    variant="outline"
                    disabled={disableBox}
                >
                    <span className="sm:hidden">Bx</span>
                    <span className="hidden sm:inline lg:hidden">Box</span>
                    <span className="hidden lg:inline">In box</span>
                </Button>
            </div>
            <PenaltyCell 
                penalty={penalties?.[0]}
                onClick={() => handlePenaltyClicked(0)} 
                className={cn("col-start-7", "border-l", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[1]}
                onClick={() => handlePenaltyClicked(1)} 
                className={cn("col-start-8", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[2]}
                onClick={() => handlePenaltyClicked(2)} 
                className={cn("col-start-9", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[3]}
                onClick={() => handlePenaltyClicked(3)} 
                className={cn("col-start-10", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[4]}
                onClick={() => handlePenaltyClicked(4)} 
                className={cn("col-start-11", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[5]}
                onClick={() => handlePenaltyClicked(5)} 
                className={cn("col-start-12", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[6]}
                onClick={() => handlePenaltyClicked(6)} 
                className={cn("col-start-13", "border-l", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[7]}
                onClick={() => handlePenaltyClicked(7)} 
                className={cn("col-start-14", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[8]}
                onClick={() => handlePenaltyClicked(8)} 
                className={cn("col-start-15", cellClass, penaltyRowClass)}
            />
            <div className={cn("col-start-16", cellClass, "border-l", penaltyRowClassFullAccent)}>                
            </div>
            <div className={cn("col-start-16", cellClass, "border-l border-r", penaltyRowClassAccent, largeNumberClass)}>  
                {penalties?.filter(p => p.penaltyCode).length}
            </div>
        </>
    )
}

type PenaltyLineupTableProps = {
    gameId: string;
}

export const PenaltyLineupTable = ({ gameId }: PenaltyLineupTableProps) => {
    const { team } = useTeamDetailsState(TeamSide.Home) ?? {};
    const { jams } = useLineupSheetState(TeamSide.Home) ?? { jams: [] };
    const gameStage = useGameStageState();
    const { sendEvent } = useEvents();
    const [penaltyDialogOpen, setPenaltyDialogOpen] = useState(false);
    const [editingSkaterNumber, setEditingSkaterNumber] = useState("");
    const [editingIndex, setEditingIndex] = useState(0);
    const [editingPenalty, setEditingPenalty] = useState<RecordedPenalty>();
    const [totalJamNumber, setTotalJamNumber] = useState(-1);

    const [skaterPenalties, setSkaterPenalties] = useState<StringMap<RecordedPenalty[]>>({});

    useEffect(() => {
        if(!team) {
            return;
        }

        setSkaterPenalties(team.roster.reduce((map, s) => ({ ...map, [s.number]: [] }), {} as StringMap<RecordedPenalty[]>));

    }, [team]);

    useEffect(() => {
        if(totalJamNumber > -1 || !gameStage) {
            return;
        }

        setTotalJamNumber(jams.findIndex(j => j.jam === gameStage.jamNumber && j.period === gameStage.periodNumber));

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
                currentJam?.jammerNumber === number ? Position.Jammer
                : currentJam?.pivotNumber === number ? Position.Pivot
                : currentJam?.blockerNumbers.includes(number) ? Position.Blocker
                : Position.Bench
            )
        }), {} as StringMap<Position>),
        [currentJam, team]);

    if(!team || !gameStage) {
        return (<></>);
    }

    const headerClass = "border-b-2 border-black";
    const headerTextClass = "font-bold flex text-center justify-center items-end";

    const handlePositionClicked = (skaterNumber: string, position: Position) => {

        if(!currentJam) {
            return;
        }

        if(position === Position.Bench) {
            sendEvent(gameId, new SkaterRemovedFromJam(
                TeamSide.Home, 
                currentJam.period,
                currentJam.jam,
                skaterNumber
            ));
        } else {
            sendEvent(gameId, new SkaterAddedToJam(
                TeamSide.Home,
                currentJam.period,
                currentJam.jam,
                position as unknown as SkaterPosition, 
                skaterNumber));
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

    const handlePenaltyAccept = (penalty: RecordedPenalty) => {
        const selectedSkaterPenalties = [...skaterPenalties[editingSkaterNumber]!];
        if(editingIndex < selectedSkaterPenalties.length) {
            selectedSkaterPenalties[editingIndex] = penalty;
        } else {
            selectedSkaterPenalties.push(penalty);
        }
        setSkaterPenalties(p => ({ ...p, [editingSkaterNumber]: selectedSkaterPenalties }));
    }

    return (
        <>
            <div className={cn(
                "w-full grid grid-flow-row grid-cols-[1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr_1fr]",
                "border-b border-black"
            )}
            >
                <div className={cn("col-start-1 flex items-end", headerClass)}>
                    <Button variant="secondary" disabled={totalJamNumber <= 0} onClick={() => setTotalJamNumber(i => i - 1)}>
                        <ChevronLeft />
                        <span className="hidden md:inline">Previous jam</span>
                    </Button>
                </div>
                {/* <div className={cn("col-start-2 col-span-5", headerClass, headerTextClass)}>
                    <span className="hidden text-xs sm:inline lg:text-base">Position</span>
                </div> */}
                <div className={cn("col-start-2 col-span-4 gap-2", headerClass, headerTextClass, "items-center md:items-end")}>
                    <span>
                        <span className="md:hidden">P</span>
                        <span className="hidden md:inline">Period </span>
                        { currentJam?.period }
                    </span>
                    <span>-</span>
                    <span>
                        <span className="md:hidden"> J</span>
                        <span className="hidden md:inline"> Jam </span>
                        { currentJam?.jam }
                    </span>
                </div>
                <div className={cn("col-start-6 flex items-end", headerClass)}>
                    <Button disabled={totalJamNumber >= jams.length - 1 || totalJamNumber === -1} onClick={() => setTotalJamNumber(i => i + 1)}>
                        <span className="hidden md:inline">Next jam</span>
                        <ChevronRight />
                    </Button>
                </div>
                <div className={cn("col-start-7 col-span-9", headerClass)}></div>
                <div className={cn("col-start-16", headerClass, headerTextClass)}>
                    <span className="hidden sm:inline 2xl:hidden text-xs lg:text-base">FO / EXP</span>
                    <span className="hidden 2xl:inline">Foul-out / Expulsion</span>
                </div>
                <div className={cn("col-start-17", headerClass, headerTextClass)}>
                <span className="hidden sm:inline 2xl:hidden text-xs lg:text-base">Total</span>
                <span className="hidden 2xl:inline">Total penalties</span>
                </div>
                { team.roster.filter(s => s.isSkating).map((s, i) => (
                    <PenaltyLineupRow 
                        key={s.number}
                        even={i % 2 == 0} 
                        skater={s} 
                        position={skaterPositions?.[s.number] ?? Position.Bench}
                        penalties={skaterPenalties[s.number]!}
                        disableBox={(currentJam?.jam ?? -1) < gameStage.jamNumber || currentJam?.period !== gameStage.periodNumber}
                        onPositionClicked={(position: Position) => handlePositionClicked(s.number, position)}
                        onPenaltyClicked={(index: number) => handlePenaltyClicked(s.number, index)} 
                    />
                ))}
            </div>
            <PenaltyDialog open={penaltyDialogOpen} currentPenalty={editingPenalty} onOpenChanged={setPenaltyDialogOpen} onAccept={handlePenaltyAccept} />
        </>
    )
}