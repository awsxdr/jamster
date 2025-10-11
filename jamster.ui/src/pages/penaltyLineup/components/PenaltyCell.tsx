import { CSSProperties } from "react";
import { cn } from "@/lib/utils";
import { Penalty } from "@/types";

type PenaltyCellProps = {
    id?: string;
    penalty?: Penalty;
    className?: string;
    style?: CSSProperties;
    compact?: boolean;
    onClick?: () => void;
}

export const PenaltyCell = ({id, penalty, className, style, compact, onClick}: PenaltyCellProps) => {
    return (
        <div
            id={id} 
            className={cn(
                "flex flex-col",
                "text-xs font-bold text-center justify-center align-center items-center cursor-pointer", 
                className,
                !compact && "xl:flex-row xl:gap-2 xl:text-sm",
            )} 
            style={style}
            onClick={onClick}
        >
            {penalty && (
                <>
                    <div>{penalty.code}</div>
                    <div>{penalty.period}-{penalty.jam}</div>
                </>
            )}
        </div>
    );
}