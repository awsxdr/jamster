import { cn } from "@/lib/utils";
import { Button } from "@/components/ui";
import { useI18n, usePenaltyBoxState } from "@/hooks";
import { Penalty, StringMap, TeamSide } from "@/types";
import { CSSProperties } from "react";

type PenaltyBoxColumnProps = {
    teamSide: TeamSide;
    skaterNumbers: string[];
    skaterPenalties: StringMap<Penalty[]>;
    compact?: boolean;
    disabled?: boolean;
    className?: string;
    onClick?: (skaterNumber: string, currentlyInBox: boolean) => void;
}

export const PenaltyBoxColumn = ({ teamSide, skaterNumbers, skaterPenalties, compact, disabled, className, onClick}: PenaltyBoxColumnProps) => {

    const { translate } = useI18n({ prefix: "PenaltyLineup.PenaltyBoxColumn." });
    const penaltyBox = usePenaltyBoxState(teamSide) ?? { skaters: [] };

    return (
        <>
        { 
            skaterNumbers.map((skaterNumber, row) => {
                const penalties = skaterPenalties[skaterNumber] ?? [];
                const inBox = penaltyBox.skaters.includes(skaterNumber);

                const rowClass = row % 2 !== 0 ? "border-black bg-blue-100 dark:bg-sky-950" : "border-black bg-blue-200 dark:bg-sky-800";

                return (
                    <div 
                        key={skaterNumber}
                        className={cn("col-start-7 row-start-[--row]", "border-b border-r-2 border-black", rowClass, className)}
                        style={{ '--row': row + 2} as CSSProperties}
                    >
                        <Button 
                            className={cn(
                                "rounded-none w-full px-1 md:px-4 border-0 h-full", 
                                penalties.some(p => !p.served) && "underline", 
                                !inBox && rowClass,
                            )}
                            variant={inBox ? "default" : "outline"}
                            disabled={disabled}
                            onClick={() => onClick?.(skaterNumber, inBox)}
                        >
                            <span className="sm:hidden">{translate("Box.Short")}</span>
                            <span className={cn("hidden sm:inline", !compact && "lg:hidden")}>{translate("Box.Medium")}</span>
                            <span className={cn("hidden", !compact && "lg:inline")}>{translate("Box.Long")}</span>
                        </Button>
                    </div>
                );
            })
        }
        </>
    );
}