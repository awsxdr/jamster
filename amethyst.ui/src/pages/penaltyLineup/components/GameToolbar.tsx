import { useState } from "react";
import { ChevronDown, ChevronUp } from "lucide-react"
import { MobileSidebarTrigger } from "@/components";
import { Button } from "@/components/ui"
import { cn } from "@/lib/utils";
import { DisplaySide, GameInfo } from "@/types";
import { GameSelectMenu, ViewMenu } from "."
import { useIsMobile } from "@/hooks";

type PltDisplayType = "None" | "Both" | "Penalties" | "Lineup";

type GameToolbarProps = {
    games: GameInfo[];
    currentGame?: GameInfo;
    selectedGameId?: string;
    displaySide: DisplaySide;
    pltDisplayType: PltDisplayType;
    onSelectedGameIdChanged: (gameId: string) => void;
    onDisplaySideChanged: (side: DisplaySide) => void;
    onPltDisplayTypeChanged: (displayTye: PltDisplayType) => void;
    disabled?: boolean;
}

export const GameToolbar = ({ games, currentGame, selectedGameId, displaySide, pltDisplayType, onSelectedGameIdChanged, onDisplaySideChanged, onPltDisplayTypeChanged, disabled }: GameToolbarProps) => {

    const isMobile = useIsMobile();
    const [showGameSelect, setShowGameSelect] = useState(true);

    return (
        <div className="flex flex-wrap flex-col justify-center md:flex-row md:flex-nowrap w-full gap-2.5 p-2">
            { isMobile &&
                <div className="flex grow justify-between">
                    <MobileSidebarTrigger />
                    <div className="flex grow justify-end gap-2">
                        <ViewMenu 
                            displaySide={displaySide} 
                            pltDisplayType={pltDisplayType} 
                            onDisplaySideChanged={onDisplaySideChanged} 
                            onPltDisplayTypeChanged={onPltDisplayTypeChanged}
                            disabled={disabled} 
                        />
                        <Button variant="ghost" className="inline" disabled={disabled} onClick={() => setShowGameSelect(v => !v)}>
                            { showGameSelect ? <ChevronUp /> : <ChevronDown /> }
                        </Button>
                    </div>
                </div>
            }
            { (!isMobile || showGameSelect) &&
                <>
                    <div className={cn("flex flex-wrap w-full justify-center bg-card gap-2", isMobile && "flex-nowrap")}>
                        <GameSelectMenu 
                            games={games} 
                            currentGame={currentGame} 
                            selectedGameId={selectedGameId} 
                            onSelectedGameIdChanged={onSelectedGameIdChanged} 
                            disabled={disabled}
                        />
                    </div>
                    { !isMobile &&
                        <div className="flex justify-end gap-2">
                            <ViewMenu 
                                displaySide={displaySide} 
                                pltDisplayType={pltDisplayType} 
                                onDisplaySideChanged={onDisplaySideChanged} 
                                onPltDisplayTypeChanged={onPltDisplayTypeChanged}
                                disabled={disabled} 
                            />
                        </div>
                    }
                </>
            }
        </div>
    )
}