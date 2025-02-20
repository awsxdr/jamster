import { cn } from "@/lib/utils";
import { Penalty } from "@/types";

type PenaltyCellProps = {
    penalty?: Penalty;
    className?: string;
    onClick?: () => void;
}

export const PenaltyCell = ({penalty, className, onClick}: PenaltyCellProps) => {
    return (
        <div 
            className={cn(
                className,
                "flex flex-col xl:flex-row xl:gap-2 text-xs xl:text-sm font-bold text-center justify-center align-center items-center cursor-pointer", 
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