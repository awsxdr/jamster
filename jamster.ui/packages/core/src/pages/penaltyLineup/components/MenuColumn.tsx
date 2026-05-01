import { CSSProperties } from "react";
import { LineupPosition, StringMap } from "@/types";
import { RowMenu } from ".";
import { Button } from "@/components/ui";
import { EllipsisVertical } from "lucide-react";

type MenuColumnProps = {
    skaterNumbers: string[];
    skaterPositions: StringMap<LineupPosition>;
    injuredSkaters: string[];
    onInjuryAdded?: (skaterNumber: string) => void;
    onInjuryRemoved?: (skaterNumber: string) => void;
}

export const MenuColumn = ({ skaterNumbers, skaterPositions, injuredSkaters, onInjuryAdded, onInjuryRemoved }: MenuColumnProps) => {
    return (
        <>
            { skaterNumbers.map((skaterNumber, row) => (
                <div 
                    key={skaterNumber}
                    className="col-start-1 row-start-[--row]" 
                    style={{ "--row": row + 2 } as CSSProperties} 
                >
                    <RowMenu 
                        disableNotes={skaterPositions[skaterNumber] === LineupPosition.Bench} 
                        injuryActive={injuredSkaters.includes(skaterNumber)}
                        onInjuryAdded={() => onInjuryAdded?.(skaterNumber)}
                        onInjuryRemoved={() => onInjuryRemoved?.(skaterNumber)}
                    >
                        <Button 
                            id={`PenaltyLineup.LineupTable.Skater${skaterNumber}.Menu`} 
                            variant="ghost" 
                            size="icon" 
                            className="hidden lg:inline px-2 text-center w-full h-full"
                        >
                            <EllipsisVertical />
                        </Button>
                    </RowMenu>
                </div>
            ))}
        </>
    );
}
