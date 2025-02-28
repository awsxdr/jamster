import { GameSkater, LineupPosition, Penalty, Ruleset } from "@/types";
import { PositionButton } from "./PositionButton";
import { cn } from "@/lib/utils";
import { Button, DropdownMenuContent, DropdownMenuItem } from "@/components/ui";
import { PenaltyCell } from "./PenaltyCell";
import { useGameStageState, useI18n } from "@/hooks";
import { Bandage, EllipsisVertical, NotebookPen, OctagonX } from "lucide-react";
import { DropdownMenu, DropdownMenuTrigger } from "@radix-ui/react-dropdown-menu";
import { PropsWithChildren } from "react";

type RowMenuProps = {
    disableNotes?: boolean;
    onInjuryAdded?: () => void;
}

const RowMenu = ({ disableNotes, onInjuryAdded, children }: PropsWithChildren<RowMenuProps>) => {
    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                { children }
            </DropdownMenuTrigger>
            <DropdownMenuContent>
                <DropdownMenuItem onClick={onInjuryAdded}>
                    <Bandage />
                    Add injury
                </DropdownMenuItem>
                <DropdownMenuItem disabled={disableNotes}>
                    <NotebookPen />
                    Notes
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
    )
}

type PenaltyLineupDisplay = "Penalties" | "Lineup" | "Both";

type PenaltyLineupRowProps = {
    skater: GameSkater;
    position: LineupPosition;
    even: boolean;
    expulsionPenalty: Penalty | null;
    penalties: Penalty[];
    rules: Ruleset;
    inBox: boolean;
    injured: boolean;
    compact?: boolean;
    display: PenaltyLineupDisplay;
    disableBox?: boolean;
    onPositionClicked?: (position: LineupPosition) => void;
    onBoxClicked?: (inBox: boolean) => void;
    onPenaltyClicked?: (index: number) => void;
    onExpulsionClicked?: () => void;
    onInjuryAdded?: () => void;
}

export const PenaltyLineupRow = ({ skater, position, even, expulsionPenalty, penalties, rules, inBox, injured, display, compact, disableBox, onPositionClicked, onBoxClicked, onPenaltyClicked, onExpulsionClicked, onInjuryAdded }: PenaltyLineupRowProps) => {

    const { translate } = useI18n({ prefix: "PenaltyLineup.PenaltyLineupRow." });
    const { periodNumber, jamNumber } = useGameStageState() ?? { periodNumber: 0, jamNumber: 0 };

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

    const penaltyCellPreviousPeriod = even
        ? "bg-gray-200 dark:bg-gray-900"
        : "bg-gray-300 dark:bg-gray-800"

    const penaltyRowClassFullAccent = "bg-red-200 dark:bg-rose-700";

    const getPenaltyCellClass = (penalty: Penalty | undefined) => 
        penalty && penalty.period < periodNumber
        ? cn(cellClass, penaltyCellPreviousPeriod, "font-light")
        : cn(cellClass, penaltyRowClass, penalty && penalty.period === periodNumber && penalty.jam === Math.max(1, jamNumber) && "underline");

    const largeNumberClass = "flex justify-center items-center text-sm sm:text-base md:text-lg";

    const buttonClass = "rounded-none w-full px-1 md:px-4 border-0 h-full";

    const handleBoxClicked = () => {
        onBoxClicked?.(!inBox);
    }

    const handlePenaltyClicked = (index: number) => {
        onPenaltyClicked?.(index);
    }

    const fouledOutOrExpelled = expulsionPenalty !== null || penalties.length >= rules.penaltyRules.foulOutPenaltyCount;

    const offTrack = injured || fouledOutOrExpelled;

    return (
        <>
            { display !== "Penalties" && (
                <div className="col-start-1">
                    <RowMenu disableNotes={position === LineupPosition.Bench} onInjuryAdded={onInjuryAdded}>
                        <Button variant="ghost" size="icon" className={cn("hidden px-2 text-center w-full h-full", !compact && "lg:inline")}>
                            <EllipsisVertical />
                        </Button>
                    </RowMenu>
                </div>
            )}
            <div className={cn("col-start-2", "border-l-2", cellClass, largeNumberClass, display === "Penalties" ? penaltyRowClassAccent : lineupRowClassAccent)}>
                <RowMenu disableNotes={position === LineupPosition.Bench} onInjuryAdded={onInjuryAdded}>
                    <Button variant="ghost" className={cn("w-full h-full text-sm sm:text-base md:text-lg p-0 font-normal flex-col justify-center items-center gap-0", !compact && "lg:hidden")}>
                        { fouledOutOrExpelled && display !== "Penalties" && <OctagonX className="text-red-600 dark:text-red-400" /> }
                        { injured && display !== "Penalties" && <Bandage className="text-yellow-600 dark:text-yellow-300" /> }
                        <span>{skater.number}</span>
                    </Button>
                </RowMenu>
                <div className={cn("hidden flex-row justify-center gap-1 items-center w-full", !compact && "lg:flex")}>
                    { fouledOutOrExpelled && display !== "Penalties" && <OctagonX className="text-red-600 dark:text-red-400" /> }
                    { injured && display !== "Penalties" && <Bandage className="text-yellow-600 dark:text-yellow-300" /> }
                    <span>{skater.number}</span>
                </div>
            </div>
            { display !== "Penalties" && (
                <>
                    <div className={cn("col-start-3", "border-l", cellClass, lineupRowClass)}>
                        <PositionButton 
                            position={position} 
                            targetPosition={LineupPosition.Bench} 
                            className={buttonClass} 
                            rowClassName={lineupRowClass}
                            compact={compact}
                            onClick={onPositionClicked}
                        />
                    </div>
                    <div className={cn("col-start-4", cellClass, lineupRowClass)}>
                        <PositionButton 
                            position={position} 
                            targetPosition={LineupPosition.Jammer} 
                            className={buttonClass} 
                            rowClassName={lineupRowClass}
                            compact={compact}
                            offTrack={offTrack}
                            onClick={onPositionClicked}
                        />
                    </div>
                    <div className={cn("col-start-5", cellClass, lineupRowClass)}>
                        <PositionButton 
                            position={position} 
                            targetPosition={LineupPosition.Pivot} 
                            className={buttonClass} 
                            rowClassName={lineupRowClass}
                            compact={compact}
                            offTrack={offTrack}
                            onClick={onPositionClicked}
                        />
                    </div>
                    <div className={cn("col-start-6", cellClass, lineupRowClass)}>
                        <PositionButton 
                            position={position} 
                            targetPosition={LineupPosition.Blocker} 
                            className={buttonClass} 
                            rowClassName={lineupRowClass}
                            compact={compact}
                            offTrack={offTrack}
                            onClick={onPositionClicked}
                        />
                    </div>
                    <div className={cn("col-start-7", cellClass, display === "Lineup" && "border-r-2", lineupRowClassAccent, "border-l")}>
                        <Button 
                            className={cn(buttonClass, penalties.some(p => !p.served) && "underline", !inBox && lineupRowClassAccent)}
                            variant={inBox ? "default" : "outline"}
                            disabled={disableBox}
                            onClick={handleBoxClicked}
                        >
                            <span className="sm:hidden">{translate("Box.Short")}</span>
                            <span className={cn("hidden sm:inline", !compact && "lg:hidden")}>{translate("Box.Medium")}</span>
                            <span className={cn("hidden", !compact && "lg:inline")}>{translate("Box.Long")}</span>
                        </Button>
                    </div>
                </>
            )}
            { display !== "Lineup" && (
                <>
                    { Array.from(Array(rules.penaltyRules.foulOutPenaltyCount + 2)).map((_, i) => (
                        <PenaltyCell 
                            key={i}
                            penalty={penalties?.[i]}
                            onClick={() => handlePenaltyClicked(i)} 
                            compact={compact}
                            className={cn(
                                display === "Both" ? `col-start-[8+${i}]` : `col-start-[3+${i}]`,
                                (i === 0 || i === rules.penaltyRules.foulOutPenaltyCount - 1) && "border-l",
                                getPenaltyCellClass(penalties?.[i]))
                            }
                        />
                    ))}
                    <PenaltyCell
                        penalty={
                            expulsionPenalty !== null ? expulsionPenalty
                            : penalties?.length >= rules.penaltyRules.foulOutPenaltyCount ? { ...penalties[rules.penaltyRules.foulOutPenaltyCount - 1], code: "FO" } 
                            : undefined
                        }
                        onClick={onExpulsionClicked}
                        compact={compact}
                        className={cn(
                            display === "Both" ? `col-start-[17]` : `col-start-[12]`,
                            cellClass,
                            "border-l",
                            penaltyRowClassFullAccent,                            
                        )}
                    />
                    <div className={cn(display === "Both" ? "col-start-18" : "col-start-13", cellClass, "border-l border-r-2", penaltyRowClassAccent, largeNumberClass)}>  
                        {penalties?.filter(p => p.code).length}
                    </div>
                </>
            )}
        </>
    )
}
