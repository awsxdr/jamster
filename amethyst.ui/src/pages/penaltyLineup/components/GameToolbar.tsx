import { useMemo, useState } from "react";
import { ChevronDown, ChevronUp } from "lucide-react"
import { MobileSidebarTrigger } from "@/components";
import { Button } from "@/components/ui"
import { cn } from "@/lib/utils";
import { DisplaySide, GameInfo } from "@/types";
import { BoxDisplayType, GameSelectMenu, PltDisplayType, ViewMenu } from "."
import { GameStateContextProvider, useGamesList, useIsMobile } from "@/hooks";

type GameToolbarProps = {
    currentGame?: GameInfo;
    selectedGameId?: string;
    displaySide: DisplaySide;
    pltDisplayType: PltDisplayType;
    boxDisplayType: BoxDisplayType;
    onSelectedGameIdChanged: (gameId: string) => void;
    onDisplaySideChanged: (side: DisplaySide) => void;
    onPltDisplayTypeChanged: (displayTye: PltDisplayType) => void;
    onBoxDisplayTypeChanged: (displayType: BoxDisplayType) => void;
    disabled?: boolean;
}

export const GameToolbar = ({ 
    currentGame, 
    selectedGameId, 
    displaySide, 
    pltDisplayType,
    boxDisplayType,
    onSelectedGameIdChanged, 
    onDisplaySideChanged, 
    onPltDisplayTypeChanged, 
    onBoxDisplayTypeChanged,
    disabled 
}: GameToolbarProps) => {

    const games = useGamesList();
    const isMobile = useIsMobile();
    const [showGameSelect, setShowGameSelect] = useState(true);

    const viewMenu = useMemo(() => selectedGameId && (
        <GameStateContextProvider gameId={selectedGameId}>
            <ViewMenu 
                displaySide={displaySide} 
                pltDisplayType={pltDisplayType}
                boxDisplayType={boxDisplayType}
                onDisplaySideChanged={onDisplaySideChanged} 
                onPltDisplayTypeChanged={onPltDisplayTypeChanged}
                onBoxDisplayTypechanged={onBoxDisplayTypeChanged}
                disabled={disabled} 
            />
        </GameStateContextProvider>
    ), [selectedGameId, displaySide, pltDisplayType, boxDisplayType, disabled]);

    return (
        <div className="flex flex-wrap flex-col justify-center md:flex-row md:flex-nowrap w-full gap-2.5 p-2">
            { isMobile &&
                <div className="flex grow justify-between">
                    <MobileSidebarTrigger />
                    <div className="flex grow justify-end gap-2">
                        { viewMenu }
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
                            { viewMenu }
                        </div>
                    }
                </>
            }
        </div>
    )
}