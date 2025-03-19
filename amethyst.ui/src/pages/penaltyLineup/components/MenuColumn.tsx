import { CSSProperties } from "react";
import { LineupPosition, StringMap } from "@/types";
import { RowMenu } from ".";
import { Button } from "@/components/ui";
import { EllipsisVertical } from "lucide-react";

type MenuColumnProps = {
    skaterNumbers: string[];
    skaterPositions: StringMap<LineupPosition>;
    onInjuryAdded?: (skaterNumber: string) => void;
}

export const MenuColumn = ({ skaterNumbers, skaterPositions, onInjuryAdded }: MenuColumnProps) => {
    return (
        <>
            { skaterNumbers.map((skaterNumber, row) => (
                <div className="col-start-1 row-start-[--row]" style={{ '--row': row + 2 } as CSSProperties} key={skaterNumber}>
                    <RowMenu 
                        disableNotes={skaterPositions[skaterNumber] === LineupPosition.Bench} 
                        onInjuryAdded={() => onInjuryAdded?.(skaterNumber)}
                    >
                        <Button variant="ghost" size="icon" className="hidden lg:inline px-2 text-center w-full h-full">
                            <EllipsisVertical />
                        </Button>
                    </RowMenu>
                </div>
            ))}
        </>
    );
}
