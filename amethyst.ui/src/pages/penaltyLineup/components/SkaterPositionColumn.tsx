import { cn } from "@/lib/utils";
import { LineupPosition } from "@/types";
import { Button } from "@/components/ui";
import { useI18n } from "@/hooks";
import { CSSProperties } from "react";

type SkaterPositionColumnProps = {
    position: LineupPosition;
    skaterNumbers: string[];
    selectedSkaters: string[];
    offTrackSkaters: string[];
    injuredSkaters: string[];
    compact?: boolean;
    className?: string;
    onSkaterClicked?: (skaterNumber: string) => void;
}

export const SkaterPositionColumn = ({ position, skaterNumbers, selectedSkaters, offTrackSkaters, injuredSkaters, compact, className, onSkaterClicked}: SkaterPositionColumnProps) => {

    const { translate } = useI18n({ prefix: "PenaltyLineup.SkaterPositionColumn." });

    return (
        <>
        { 
            skaterNumbers.map((skaterNumber, row) => {
                const selected = selectedSkaters.some(s => s === skaterNumber);
                const variant =
                    selected && position !== LineupPosition.Bench && (offTrackSkaters.includes(skaterNumber) || injuredSkaters.includes(skaterNumber)) ? "destructive"
                    : selected ? "default" 
                    : "outline";

                const rowClass = row % 2 === 0 ? "bg-white dark:bg-gray-900" : "bg-blue-100 dark:bg-sky-950";

                return (
                    <div 
                        key={skaterNumber}
                        className={cn(
                            "border-l border-b border-black",
                            "row-start-[--row]",
                            rowClass,
                            className,
                        )}
                        style={{ '--row': row + 2} as CSSProperties}
                    >
                        <Button 
                            className={cn("rounded-none w-full px-1 md:px-4 border-0 h-full", !selected && rowClass)}
                            variant={variant}
                            onClick={() => onSkaterClicked?.(skaterNumber)}
                        >
                            <span className={cn(!compact && "lg:hidden")}>{translate(`${position}.Short`)}</span>
                            <span className={cn("hidden", !compact && "lg:inline")}>{translate(`${position}.Long`)}</span>
                        </Button>
                    </div>
                );
            })
        }
        </>
    );
}