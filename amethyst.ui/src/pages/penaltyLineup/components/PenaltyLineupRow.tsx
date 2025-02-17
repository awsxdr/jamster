import { GameSkater, LineupPosition } from "@/types";
import { RecordedPenalty } from "./PenaltyDialog";
import { PositionButton } from "./PositionButton";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui";
import { PenaltyCell } from "./PenaltyCell";
import { useI18n } from "@/hooks";

type PenaltyLineupRowProps = {
    skater: GameSkater;
    position: LineupPosition;
    even: boolean;
    penalties: RecordedPenalty[];
    disableBox?: boolean;
    onPositionClicked?: (position: LineupPosition) => void;
    onPenaltyClicked?: (index: number) => void;
}

export const PenaltyLineupRow = ({ skater, position, even, penalties, disableBox, onPositionClicked, onPenaltyClicked }: PenaltyLineupRowProps) => {

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
                    targetPosition={LineupPosition.Bench} 
                    className={buttonClass} 
                    rowClassName={lineupRowClass}
                    onClick={onPositionClicked}
                />
            </div>
            <div className={cn("col-start-3", cellClass, lineupRowClass)}>
                <PositionButton 
                    position={position} 
                    targetPosition={LineupPosition.Jammer} 
                    className={buttonClass} 
                    rowClassName={lineupRowClass}
                    onClick={onPositionClicked}
                />
            </div>
            <div className={cn("col-start-4", cellClass, lineupRowClass)}>
                <PositionButton 
                    position={position} 
                    targetPosition={LineupPosition.Pivot} 
                    className={buttonClass} 
                    rowClassName={lineupRowClass}
                    onClick={onPositionClicked}
                />
            </div>
            <div className={cn("col-start-5", cellClass, lineupRowClass)}>
                <PositionButton 
                    position={position} 
                    targetPosition={LineupPosition.Blocker} 
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
                    <span className="sm:hidden">{translate("Box.Short")}x</span>
                    <span className="hidden sm:inline lg:hidden">{translate("Box.Medium")}</span>
                    <span className="hidden lg:inline">{translate("Box.Long")}</span>
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
