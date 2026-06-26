/* eslint-disable @stylistic/indent */
import { cn } from "@/lib/utils";
import { GameSkater, LineupPosition, TeamSide } from "@/types";
import { Button, ButtonVariant } from "@/components/ui";
import { useI18n, usePenaltyBoxState } from "@/hooks";
import { CSSProperties } from "react";
import { switchex } from "@/utilities/switchex";

type SkaterPositionColumnProps = {
    teamSide: TeamSide;
    position: LineupPosition;
    skaters: GameSkater[];
    selectedSkaters: string[];
    offTrackSkaters: string[];
    injuredSkaters: string[];
    currentJam?: boolean;
    compact?: boolean;
    className?: string;
    onSkaterClicked?: (skaterNumber: string) => void;
}

export const SkaterPositionColumn = ({ teamSide, position, skaters, selectedSkaters, offTrackSkaters, injuredSkaters, currentJam, compact, className, onSkaterClicked}: SkaterPositionColumnProps) => {

    const { translate } = useI18n({ prefix: "PenaltyLineup.SkaterPositionColumn." });
    const penaltyBox = usePenaltyBoxState(teamSide) ?? { skaters: [], queuedSkaters: [] };

    return (
        <>
            { 
                skaters.map(({ id }, row) => {
                    const selected = selectedSkaters.some(s => s === id);
                    const inBox = penaltyBox.skaters.includes(id);

                    const variant =
                        !selected ? "outline"
                        : !currentJam ? "default"
                        : switchex(position)
                            .predicate(p => p !== LineupPosition.Bench && (offTrackSkaters.includes(id) || injuredSkaters.includes(id))).then<ButtonVariant>("destructive")
                            .predicate(p => p === LineupPosition.Bench && inBox).then("destructive")
                            .default("default");

                    const even = row % 2 === 0;
                    const isBench = position === LineupPosition.Bench;
                    const rowClass = even 
                        ? (isBench ? "bg-gray-200 dark:bg-gray-700" : "bg-white dark:bg-gray-900") 
                        : (isBench ? "bg-gray-300 dark:bg-gray-600" : "bg-blue-100 dark:bg-sky-950");

                    return (
                        <div 
                            key={id}
                            className={cn(
                                "border-l border-b border-black",
                                "row-start-[--row]",
                                rowClass,
                                className,
                            )}
                            style={{ '--row': row + 2} as CSSProperties}
                        >
                            <Button 
                                id={`PenaltyLineup.LineupTable.Skater${id}.Position.${position}`}
                                aria-label={position}
                                aria-invalid={variant.toString() === "destructive"}
                                className={cn("rounded-none w-full px-1 md:px-4 border-0 h-full", !selected && rowClass)}
                                variant={variant}
                                onClick={() => onSkaterClicked?.(id)}
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