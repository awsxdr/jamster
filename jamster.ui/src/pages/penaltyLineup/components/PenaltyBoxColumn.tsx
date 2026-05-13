import { cn } from "@/lib/utils";
import { Button } from "@/components/ui";
import { useI18n, usePenaltyBoxState } from "@/hooks";
import { GameSkater, Penalty, StringMap, TeamSide } from "@/types";
import { CSSProperties } from "react";

type PenaltyBoxColumnProps = {
    teamSide: TeamSide;
    skaters: GameSkater[];
    skaterPenalties: StringMap<Penalty[]>;
    compact?: boolean;
    disabled?: boolean;
    className?: string;
    onClick?: (skaterNumber: string, currentlyInBox: boolean) => void;
}

export const PenaltyBoxColumn = ({ teamSide, skaters, skaterPenalties, compact, disabled, className, onClick}: PenaltyBoxColumnProps) => {

    const { translate } = useI18n({ prefix: "PenaltyLineup.PenaltyBoxColumn." });
    const penaltyBox = usePenaltyBoxState(teamSide) ?? { skaters: [], queuedSkaters: [] };

    return (
        <>
            { 
                skaters.map(({ id }, row) => {
                    const penalties = skaterPenalties[id] ?? [];
                    const inBox = penaltyBox.skaters.includes(id);

                    const even = row % 2 === 0;
                    const expectedInBox = penalties.some(p => !p.served);
                    const rowClass = even 
                        ? cn("border-black", expectedInBox ? "bg-yellow-200 dark:bg-yellow-800 font-bold" : "bg-blue-200 dark:bg-sky-800")
                        : cn("border-black", expectedInBox ? "bg-yellow-100 dark:bg-yellow-900 font-bold" : "bg-blue-100 dark:bg-sky-950");

                    return (
                        <div 
                            key={id}
                            className={cn("col-start-7 row-start-[--row]", "border-b border-r-2 border-black", rowClass, className)}
                            style={{ '--row': row + 2} as CSSProperties}
                        >
                            <Button 
                                id={`PenaltyLineup.LineupTable.Skater${id}.InBox`}
                                className={cn(
                                    "rounded-none w-full px-1 md:px-4 border-0 h-full", 
                                    !inBox && rowClass,
                                )}
                                variant={inBox ? "default" : "outline"}
                                disabled={disabled}
                                onClick={() => onClick?.(id, inBox)}
                                aria-checked={inBox}
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