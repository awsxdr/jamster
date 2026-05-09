import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuPortal, DropdownMenuSub, DropdownMenuSubContent, DropdownMenuSubTrigger, DropdownMenuTrigger, Slider } from "@/components/ui"
import { useEvents, useI18n } from "@/hooks";
import { cn } from "@/lib/utils";
import { TeamSide } from "@/types";
import { ScoreSheetLineDeleted, ScoreSheetStarPassTripSet } from "@/types/events";
import { Check, EllipsisVertical, Star, Trash } from "lucide-react"
import React, { MouseEvent, useEffect, useState } from "react";

type JamEditMenuProps = {
    gameId: string;
    teamSide: TeamSide;
    lineNumber: number;
    starPassTrip: number | null;
    className?: string;
}

export const JamEditMenu = React.memo(function JamEditMenu({ gameId, teamSide, lineNumber, starPassTrip, className }: JamEditMenuProps) {

    const { sendEvent } = useEvents();

    const [starPassValue, setStarPassValue] = useState(1);
    const [starPass, setStarPass] = useState(false);
    const { translate } = useI18n({ prefix: "ScoreboardControl.StatsSheet.JamEditMenu." });
    
    useEffect(() => {
        setStarPassValue(starPassTrip === null ? 0 : starPassTrip + 1);
        setStarPass(starPassTrip !== null);
    }, [starPassTrip]);

    const handleStarPassClick = (event: MouseEvent<HTMLDivElement>) => {
        event.preventDefault();
        const newStarPass = !starPass;
        setStarPass(newStarPass);

        sendEvent(gameId, new ScoreSheetStarPassTripSet(teamSide, lineNumber, newStarPass ? 0 : null));
    }

    const handleStarPassTripCommit = () => {
        sendEvent(gameId, new ScoreSheetStarPassTripSet(teamSide, lineNumber, starPassValue - 1));
    }

    const handleJamDeleted = () => {
        sendEvent(gameId, new ScoreSheetLineDeleted(lineNumber));
    }

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button className={cn("col-start-1 h-full w-full", className)} variant="ghost" size="icon">
                    <EllipsisVertical />
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-48">
                <DropdownMenuItem onClick={handleStarPassClick}><Star /> {translate("StarPass")} {starPass && <Check />}</DropdownMenuItem>
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
                        <Trash /> {translate("Delete")}
                    </DropdownMenuSubTrigger>
                    <DropdownMenuPortal>
                        <DropdownMenuSubContent>
                            <DropdownMenuItem className="bg-destructive text-destructive-foreground" onClick={handleJamDeleted}>
                                {translate("ConfirmDelete")}
                            </DropdownMenuItem>
                        </DropdownMenuSubContent>
                    </DropdownMenuPortal>
                </DropdownMenuSub>
            </DropdownMenuContent>
        </DropdownMenu>
    )
});