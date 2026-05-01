import { Penalty } from "@/types";
import { PenaltyCell } from ".";
import { useGameStageState, useRulesState } from "@/hooks";
import { CSSProperties } from "react";
import { cn } from "@/lib/utils";
import { switchex } from "@/utilities/switchex";

type PenaltyRowProps = {
    skaterNumber: string;
    penalties: Penalty[];
    expulsionPenalty: Penalty | null;
    offsetForLineupTable?: boolean;
    row: number;
    compact?: boolean;
    onPenaltyClicked?: (index: number) => void;
    onExpulsionClicked?: () => void;
}

export const PenaltyRow = ({ skaterNumber, penalties, expulsionPenalty, offsetForLineupTable, row, compact, onPenaltyClicked, onExpulsionClicked }: PenaltyRowProps) => {

    const { rules } = useRulesState() ?? { };
    const { periodNumber, jamNumber } = useGameStageState() ?? { periodNumber: 0, jamNumber: 0 };

    if(!rules) {
        return (
            <></>
        );
    }

    const cellClass = "border-b border-r border-black";

    const even = row % 2 === 0;

    const penaltyRowClass = even
        ? "bg-white dark:bg-rose-950"
        : "bg-red-100 dark:bg-rose-900";

    const rowClassOverride = switchex(penalties.length)
        .predicate(() => !!expulsionPenalty).then("bg-red-700 text-white")
        .predicate(l => l >= rules.penaltyRules.foulOutPenaltyCount).then("bg-red-700 text-white")
        .case(rules.penaltyRules.foulOutPenaltyCount - 1).then("bg-orange-600 text-white dark:bg-orange-700")
        .case(rules.penaltyRules.foulOutPenaltyCount - 2).then("bg-yellow-400 text-black dark:bg-yellow-600")
        .default(undefined);

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
            : cn(cellClass, rowClassOverride ?? penaltyRowClass, penalty && penalty.period === periodNumber && penalty.jam === Math.max(1, jamNumber) && "underline");
    
    if (!rules) {
        return (<></>);
    }

    return (
        <>
            { !offsetForLineupTable && (
                <div 
                    className={cn(
                        "col-start-2 row-start-[--row]",
                        cellClass,
                        "border-l-2 border-r-2",
                        penaltyRowClassAccent,
                        "flex justify-center items-center text-sm sm:text-base md:text-lg"
                    )}
                    style={{ 
                        '--row': row + 2 
                    } as CSSProperties}
                > 
                    {skaterNumber}
                </div>
            )}
            { Array.from(Array(rules.penaltyRules.foulOutPenaltyCount + 2)).map((_, i) => {
                return (
                    <PenaltyCell 
                        id={`PenaltyLineup.PenaltyRow.${skaterNumber}.${i + 1}`}
                        key={i}
                        penalty={penalties[i]}
                        compact={compact}
                        className={cn(
                            "col-start-[--col] row-start-[--row]",
                            i === rules.penaltyRules.foulOutPenaltyCount - 1 && "border-l",
                            getPenaltyCellClass(penalties?.[i]))
                        }
                        style={{ 
                            '--col': i + (offsetForLineupTable ? 8 : 3),
                            '--row': row + 2 
                        } as CSSProperties}
                        onClick={() => onPenaltyClicked?.(i)} 
                    />
                );
            })}
            <PenaltyCell
                penalty={
                    expulsionPenalty !== null 
                        ? expulsionPenalty
                        : penalties?.length >= rules.penaltyRules.foulOutPenaltyCount 
                            ? { ...penalties[rules.penaltyRules.foulOutPenaltyCount - 1], code: "FO" } 
                            : undefined
                }
                onClick={onExpulsionClicked}
                compact={compact}
                className={cn(
                    offsetForLineupTable ? `col-start-[17]` : `col-start-[12]`,
                    "row-start-[--row]",
                    cellClass,
                    "border-l",
                    penaltyRowClassFullAccent,                            
                )}
                style={{ 
                    '--row': row + 2 
                } as CSSProperties}
            />
            <div 
                className={cn(
                    offsetForLineupTable ? "col-start-[18]" : "col-start-[13]",
                    "row-start-[--row]",
                    cellClass,
                    "border-l border-r-2",
                    penaltyRowClassAccent,
                    "flex justify-center items-center text-sm sm:text-base md:text-lg"
                )}
                style={{ 
                    '--row': row + 2 
                } as CSSProperties}
            > 
                {penalties?.filter(p => p.code).length}
            </div>
        </>
    );
}