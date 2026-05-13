import { useRulesState } from "@/hooks";
import { GameSkater, LineupPosition, Penalty, StringMap } from "@/types";
import { Bandage, OctagonX } from "lucide-react";
import { RowMenu } from ".";
import { Button } from "@/components/ui";
import { cn } from "@/lib/utils";
import { CSSProperties, Fragment } from "react";

type SkaterNumberColumnProps = {
    skaters: GameSkater[];
    skaterPositions: StringMap<LineupPosition>;
    skaterPenalties: StringMap<Penalty[]>;
    offTrackSkaters: string[];
    injuredSkaters: string[];
    compact?: boolean;
    onInjuryAdded?: (skaterNumber: string) => void;
    onInjuryRemoved?: (skaterNumber: string) => void;
}

export const SkaterNumberColumn = ({ skaters, skaterPositions, skaterPenalties, offTrackSkaters, injuredSkaters, compact, onInjuryAdded, onInjuryRemoved }: SkaterNumberColumnProps) => {

    const { rules } = useRulesState() ?? { };

    if(!rules) {
        return (<></>);
    }

    return (
        <>
            { skaters.map(({ id, number }, row) => {
                const position = skaterPositions[id];
                const penalties = skaterPenalties[id];
                const injured = injuredSkaters.includes(id);

                if (!penalties) {
                    return (<Fragment key={id}></Fragment>);
                }

                const content = (
                    <>
                        { offTrackSkaters.includes(id) && <OctagonX className="text-red-600 dark:text-red-400" /> }
                        { injured && <Bandage className="text-yellow-600 dark:text-yellow-300" /> }
                        <span>{number}</span>
                    </>);

                return (
                    <div 
                        key={id} 
                        className={cn(
                            "col-start-2 row-start-[--row]",
                            "border-l-2 border-b border-r border-black",
                            "flex justify-center items-center text-sm sm:text-base md:text-lg",
                            row % 2 === 0 ? "border-black bg-blue-100 dark:bg-sky-950" : "border-black bg-blue-200 dark:bg-sky-800"
                        )}
                        style={{
                            '--row': row + 2
                        } as CSSProperties}
                    >
                        <RowMenu 
                            disableNotes={position === LineupPosition.Bench} 
                            injuryActive={injuredSkaters.includes(id)}
                            onInjuryAdded={() => onInjuryAdded?.(id)}
                            onInjuryRemoved={() => onInjuryRemoved?.(id)}
                        >
                            <Button variant="ghost" className={cn("w-full h-full text-sm sm:text-base md:text-lg p-0 font-normal flex-col justify-center items-center gap-0", !compact && "lg:hidden")}>
                                {content}
                            </Button>
                        </RowMenu>
                        <div className={cn("hidden flex-row justify-center gap-1 items-center w-full", !compact && "lg:flex")}>
                            {content}
                        </div>
                    </div>
                );
            })}
        </>
    )
}
