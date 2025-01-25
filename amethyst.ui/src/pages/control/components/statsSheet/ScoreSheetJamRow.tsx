import { cn } from "@/lib/utils";
import { ScoreSheetJam } from "@/types"

type ScoreSheetJamRowProps = {
    line: ScoreSheetJam;
    even: boolean;
}

export const ScoreSheetJamRow = ({ line, even }: ScoreSheetJamRowProps) => {

    const rowColorClass = even ? "bg-white dark:bg-gray-900" : "bg-green-100 dark:bg-green-900";
    const rowEmphasisColorClass = even ? "bg-green-100 dark:bg-green-900" : "bg-green-200 dark:bg-green-700";
    const rowFixedEmphasisColorClass = "bg-green-200 dark:bg-green-700";

    const numberCellClass = "text-center border-l border-b border-black dark:border-gray-400";
    const checkCellClass = "text-center border-l border-b border-black dark:border-gray-400";

    return (
        <>
            <div className={cn("col-start-1", numberCellClass, rowEmphasisColorClass)}>{line.lineLabel}</div>
            <div className={cn("col-start-2", numberCellClass, rowColorClass)}>{line.jammerNumber}</div>
            <div className={cn("col-start-3", checkCellClass, "border-l-2", rowEmphasisColorClass)}>{line.lost ? "X" : ""}</div>
            <div className={cn("col-start-4", checkCellClass, "border-black", rowEmphasisColorClass)}>{line.lead ? "X" : ""}</div>
            <div className={cn("col-start-5", checkCellClass, "border-black", rowEmphasisColorClass)}>{line.called ? "X" : ""}</div>
            <div className={cn("col-start-6", checkCellClass, "border-black", rowEmphasisColorClass)}>{line.injury ? "X" : ""}</div>
            <div className={cn("col-start-7", checkCellClass, "border-black", "border-r border-black", rowEmphasisColorClass)}>{line.noInitial ? "X" : ""}</div>
            { Array.from(new Array(9)).map((_, i) => (
                <div className={cn(`col-start-${i + 8}`, numberCellClass, rowColorClass)}>{line.trips[i]?.score}</div>
            ))}
            <div className={cn("col-start-17", numberCellClass, "border-l-2", rowColorClass)}>{line.jamTotal}</div>
            <div className={cn("col-start-18", numberCellClass, "border-l-2", rowFixedEmphasisColorClass)}>{line.gameTotal}</div>
        </>
    )
}