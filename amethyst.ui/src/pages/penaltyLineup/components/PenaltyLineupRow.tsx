import { GameSkater, LineupPosition, Penalty } from "@/types";
import { PositionButton } from "./PositionButton";
import { cn } from "@/lib/utils";
import { Button, DropdownMenuContent, DropdownMenuItem } from "@/components/ui";
import { PenaltyCell } from "./PenaltyCell";
import { useI18n } from "@/hooks";
import { Bandage, EllipsisVertical, NotebookPen } from "lucide-react";
import { DropdownMenu, DropdownMenuTrigger } from "@radix-ui/react-dropdown-menu";
import { PropsWithChildren } from "react";

type RowMenuProps = {
    disableNotes?: boolean;
}

const RowMenu = ({ disableNotes, children }: PropsWithChildren<RowMenuProps>) => {
    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                { children }
            </DropdownMenuTrigger>
            <DropdownMenuContent>
                <DropdownMenuItem>
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

type PenaltyLineupRowProps = {
    skater: GameSkater;
    position: LineupPosition;
    even: boolean;
    penalties: Penalty[];
    inBox: boolean;
    injured: boolean;
    compact?: boolean;
    disableBox?: boolean;
    onPositionClicked?: (position: LineupPosition) => void;
    onBoxClicked?: (inBox: boolean) => void;
    onPenaltyClicked?: (index: number) => void;
    onInjuryAdded?: () => void;
}

export const PenaltyLineupRow = ({ skater, position, even, penalties, inBox, injured, compact, disableBox, onPositionClicked, onBoxClicked, onPenaltyClicked }: PenaltyLineupRowProps) => {

    const { translate } = useI18n({ prefix: "PenaltyLineup.PenaltyLineupRow." })

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

    const buttonClass = "rounded-none w-full px-1 md:px-4 border-0 h-full";

    const handleBoxClicked = () => {
        onBoxClicked?.(!inBox);
    }

    const handlePenaltyClicked = (index: number) => {
        onPenaltyClicked?.(index);
    }

    return (
        <>
            <div className="col-start-1">
                <RowMenu disableNotes={position === LineupPosition.Bench}>
                    <Button variant="ghost" size="icon" className={cn("hidden px-2 text-center w-full h-full", !compact && "lg:inline")}>
                        <EllipsisVertical />
                    </Button>
                </RowMenu>
            </div>
            <div className={cn("col-start-2", "border-l-2", cellClass, largeNumberClass, lineupRowClassAccent)}>
                <RowMenu disableNotes={position === LineupPosition.Bench}>
                    <Button variant="ghost" className={cn("w-full h-full text-sm sm:text-base md:text-lg p-0 font-normal flex-col justify-center items-center gap-0", !compact && "lg:hidden")}>
                        { injured && <Bandage className="text-yellow-600" /> }
                        <span>{skater.number}</span>
                    </Button>
                </RowMenu>
                <div className={cn("hidden flex-row justify-center gap-1 items-center w-full", !compact && "lg:flex")}>
                    { injured && <Bandage className="text-yellow-600" /> }
                    <span>{skater.number}</span>
                </div>
            </div>
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
                    onClick={onPositionClicked}
                />
            </div>
            <div className={cn("col-start-7", cellClass, lineupRowClassAccent, "border-l")}>
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
            <PenaltyCell 
                penalty={penalties?.[0]}
                onClick={() => handlePenaltyClicked(0)} 
                compact={compact}
                className={cn("col-start-8", "border-l", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[1]}
                onClick={() => handlePenaltyClicked(1)} 
                className={cn("col-start-9", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[2]}
                onClick={() => handlePenaltyClicked(2)} 
                className={cn("col-start-10", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[3]}
                onClick={() => handlePenaltyClicked(3)} 
                className={cn("col-start-11", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[4]}
                onClick={() => handlePenaltyClicked(4)} 
                className={cn("col-start-12", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[5]}
                onClick={() => handlePenaltyClicked(5)} 
                className={cn("col-start-13", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[6]}
                onClick={() => handlePenaltyClicked(6)} 
                className={cn("col-start-14", "border-l", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[7]}
                onClick={() => handlePenaltyClicked(7)} 
                className={cn("col-start-15", cellClass, penaltyRowClass)}
            />
            <PenaltyCell 
                penalty={penalties?.[8]}
                onClick={() => handlePenaltyClicked(8)} 
                className={cn("col-start-16", cellClass, penaltyRowClass)}
            />
            <div className={cn("col-start-17", cellClass, "border-l", penaltyRowClassFullAccent)}>                
            </div>
            <div className={cn("col-start-18", cellClass, "border-l border-r-2", penaltyRowClassAccent, largeNumberClass)}>  
                {penalties?.filter(p => p.code).length}
            </div>
        </>
    )
}
