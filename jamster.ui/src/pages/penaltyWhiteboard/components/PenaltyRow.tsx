import { useRulesState } from "@/hooks";
import { cn } from "@/lib/utils";
import { PenaltySheetLine } from "@/types";
import { switchex } from "@/utilities/switchex";
import { CrossCircledIcon, ExclamationTriangleIcon } from "@radix-ui/react-icons";
import { CSSProperties } from "react";

type PenaltyRowProps = PenaltySheetLine & {
    row: number;
}

export const PenaltyRow = ({ skaterNumber, expulsionPenalty, penalties, row }: PenaltyRowProps) => {

    const { rules } = useRulesState() ?? { };

    if(!rules) {
        return (
            <></>
        );
    }

    const even = row % 2 === 0;

    const rowClassOverride =
        switchex(penalties.length)
            .predicate(l => !!expulsionPenalty || l >= rules.penaltyRules.foulOutPenaltyCount).then("bg-red-700 text-white")
            .case(rules.penaltyRules.foulOutPenaltyCount - 1).then("bg-orange-600 text-white")
            .case(rules.penaltyRules.foulOutPenaltyCount - 2).then("bg-yellow-400 text-black")
            .default(undefined);

    const penaltyRowClass = 
        even 
            ? "bg-white dark:bg-rose-950"
            : "bg-red-100 dark:bg-rose-900";

    const penaltyRowClassAccent =
        even
            ? "bg-red-100 dark:bg-rose-900"
            : "bg-red-200 dark:bg-rose-700";

    const penaltyRowClassFullAccent = "bg-red-200 dark:bg-rose-700";

    return (
        <>
            <div 
                className={cn(
                    "col-start-1 row-start[--row]",
                    "border-b border-l-2 border-r-2 border-black bg-white",
                    "py-1 lg:py-2",
                    "flex flex-col xl:flex-row justify-center items-center gap-1",
                    "text-sm sm:text-base md:text-lg",
                    penaltyRowClassAccent,
                )}
                style={{
                    '--row': row + 2
                } as CSSProperties}
            >
                { (expulsionPenalty || penalties.length >= rules.penaltyRules.foulOutPenaltyCount) && (<CrossCircledIcon className="text-red-600" />)}
                { penalties.length === rules.penaltyRules.foulOutPenaltyCount - 1 && (<ExclamationTriangleIcon className="text-orange-600" />)}
                {skaterNumber}
            </div>
            { Array.from(Array(rules.penaltyRules.foulOutPenaltyCount + 2)).map((_, i) => (
                <div
                    key={i}
                    className={cn(
                        "flex flex-col",
                        "border-r border-b border-black",
                        "col-start-[--col] row-start-[--row]",
                        i === rules.penaltyRules.foulOutPenaltyCount - 1 && "border-l",
                        "font-bold text-center justify-center align-center items-center", 
                        "text-base 2xl:flex-row 2xl:gap-2 2xl:text-lg",
                        rowClassOverride ?? penaltyRowClass,
                    )} 
                    style={{ 
                        '--col': i + 2,
                        '--row': row + 2 
                    } as CSSProperties}
                >
                    {penalties?.[i] && (
                        <>
                            <div>{penalties[i].code}</div>
                            <div>{penalties[i].period}-{penalties[i].jam}</div>
                        </>
                    )}
                </div>
            ))}
            <div 
                className={cn(
                    "flex flex-col",
                    "border-r border-b border-black",
                    "col-start-[--col] row-start-[--row]",
                    "font-bold text-center justify-center align-center items-center", 
                    "text-base 2xl:flex-row 2xl:gap-2 2xl:text-lg",
                    rowClassOverride ?? penaltyRowClassFullAccent,
                )} 
                style={{ 
                    '--col': 11,
                    '--row': row + 2 
                } as CSSProperties}
            >
                {expulsionPenalty && (
                    <>
                        <div>{expulsionPenalty.code}</div>
                        <div>{expulsionPenalty.period}-{expulsionPenalty.jam}</div>
                    </>
                )}
                {!expulsionPenalty && penalties.length >= rules.penaltyRules.foulOutPenaltyCount && (
                    <>
                        <div>FO</div>
                        <div>{penalties[rules.penaltyRules.foulOutPenaltyCount - 1].period}-{penalties[rules.penaltyRules.foulOutPenaltyCount - 1].jam}</div>
                    </>
                )}
            </div>
            <div 
                className={cn(
                    "flex flex-col",
                    "border-r-2 border-b border-black",
                    "col-start-[--col] row-start-[--row]",
                    "font-bold text-center justify-center align-center items-center", 
                    "text-base xl:flex-row xl:gap-2 xl:text-lg",
                    rowClassOverride ?? penaltyRowClassAccent,
                )} 
                style={{ 
                    '--col': 12,
                    '--row': row + 2 
                } as CSSProperties}
            >
                {penalties?.filter(p => p.code).length}
            </div>
        </>
    )
}