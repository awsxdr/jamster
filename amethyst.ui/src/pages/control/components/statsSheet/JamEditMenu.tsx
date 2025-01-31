import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuPortal, DropdownMenuSub, DropdownMenuSubContent, DropdownMenuSubTrigger, DropdownMenuTrigger, Slider } from "@/components/ui"
import { cn } from "@/lib/utils";
import { Check, EllipsisVertical, Star, Trash } from "lucide-react"
import { MouseEvent, useEffect, useState } from "react";

type JamEditMenuProps = {
    starPassTrip: number | null;
    className?: string;
    onStarPassTripChanged?: (starPassTrip: number | null) => void;
}

export const JamEditMenu = ({ starPassTrip, className, onStarPassTripChanged }: JamEditMenuProps) => {

    const [starPassValue, setStarPassValue] = useState(1);
    const [starPass, setStarPass] = useState(false);

    useEffect(() => {
        setStarPassValue(starPassTrip === null ? 0 : starPassTrip + 1);
        setStarPass(starPassTrip !== null);
    }, [starPassTrip]);

    const handleStarPassClick = (event: MouseEvent<HTMLDivElement>) => {
        event.preventDefault();
        const newStarPass = !starPass;
        setStarPass(newStarPass);
        onStarPassTripChanged?.(newStarPass ? 0 : null);
    }

    const handleStarPassTripCommit = () => {
        onStarPassTripChanged?.(starPassValue - 1);
    }

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button className={cn("col-start-1 h-full w-full", className)} variant="ghost" size="icon">
                    <EllipsisVertical />
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-48">
                <DropdownMenuItem onClick={handleStarPassClick}><Star /> Star pass {starPass && <Check />}</DropdownMenuItem>
                { starPass && (
                    <DropdownMenuItem onClick={e => e.preventDefault()}>
                        <Slider 
                            disabled={!starPass} 
                            min={1} 
                            max={10} 
                            value={[starPassValue]} 
                            onValueChange={v => setStarPassValue(v[0])} 
                            onValueCommit={handleStarPassTripCommit}
                        />
                        <span className="w-3 text-center">{starPassValue}</span>
                    </DropdownMenuItem>
                )}
                <DropdownMenuSub>
                    <DropdownMenuSubTrigger>
                        <Trash /> Delete
                    </DropdownMenuSubTrigger>
                    <DropdownMenuPortal>
                        <DropdownMenuSubContent>
                            <DropdownMenuItem className="bg-destructive text-destructive-foreground">Confirm</DropdownMenuItem>
                        </DropdownMenuSubContent>
                    </DropdownMenuPortal>
                </DropdownMenuSub>
            </DropdownMenuContent>
        </DropdownMenu>
    )
}