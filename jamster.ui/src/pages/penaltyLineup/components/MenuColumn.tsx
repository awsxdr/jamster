import { CSSProperties } from "react";
import { GameSkater, LineupPosition, StringMap, TeamSide } from "@/types";
import { RowMenu } from ".";
import { Button } from "@/components/ui";
import { EllipsisVertical } from "lucide-react";
import { usePenaltyBoxState } from "@/hooks";

export type MenuColumnProps = {
    teamSide: TeamSide;
    skaters: GameSkater[];
    skaterPositions: StringMap<LineupPosition>;
    injuredSkaters: string[];
    onInjuryAdded?: (skaterNumber: string) => void;
    onInjuryRemoved?: (skaterNumber: string) => void;
}

export const MenuColumn = ({ teamSide, skaters, skaterPositions, injuredSkaters, onInjuryAdded, onInjuryRemoved }: MenuColumnProps) => {

    const penaltyBox = usePenaltyBoxState(teamSide) ?? { skaters: [], queuedSkaters: [] };

    return (
        <>
            { skaters.map(({ id }, row) => {
                const inBox = penaltyBox.skaters.includes(id);

                return (
                    <div 
                        key={id}
                        className="col-start-1 row-start-[--row]" 
                        style={{ "--row": row + 2 } as CSSProperties} 
                    >
                        <RowMenu 
                            inBox={inBox}
                            disableNotes={skaterPositions[id] === LineupPosition.Bench} 
                            injuryActive={injuredSkaters.includes(id)}
                            onInjuryAdded={() => onInjuryAdded?.(id)}
                            onInjuryRemoved={() => onInjuryRemoved?.(id)}
                        >
                            <Button 
                                id={`PenaltyLineup.LineupTable.Skater${id}.Menu`} 
                                variant="ghost" 
                                size="icon" 
                                className="hidden lg:inline px-2 text-center w-full h-full"
                            >
                                <EllipsisVertical />
                            </Button>
                        </RowMenu>
                    </div>
                )
            })}
        </>
    );
}
