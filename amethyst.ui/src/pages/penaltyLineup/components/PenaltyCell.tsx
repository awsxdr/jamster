import { cn } from "@/lib/utils";
import { Penalty } from "@/types";

type PenaltyCellProps = {
    penalty?: Penalty;
    className?: string;
    compact?: boolean;
    onClick?: () => void;
}

export const PenaltyCell = ({penalty, className, compact, onClick}: PenaltyCellProps) => {
    return (
        <div 
            className={cn(
                "flex flex-col",
                "text-xs font-bold text-center justify-center align-center items-center cursor-pointer", 
                className,
                !compact && "xl:flex-row xl:gap-2 xl:text-sm",
            )} 
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