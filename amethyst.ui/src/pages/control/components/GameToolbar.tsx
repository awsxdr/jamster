import { useState } from "react";
import { ChevronDown, ChevronUp, Repeat, SquarePlus } from "lucide-react"
import { MobileSidebarTrigger, NewGameDialogTrigger } from "@/components";
import { Button, useSidebar } from "@/components/ui"
import { useI18n } from "@/hooks";
import { cn } from "@/lib/utils";
import { GameInfo } from "@/types";
import { ConfirmMakeCurrentDialog, GameSelectMenu, SettingsMenu, UserMenu, ViewMenu } from "."

type GameToolbarProps = {
    games: GameInfo[];
    currentGame?: GameInfo;
    onCurrentGameIdChanged: (gameId: string) => void;
    selectedGameId?: string;
    onSelectedGameIdChanged: (gameId: string) => void;
    disabled?: boolean;
}

export const GameToolbar = ({ games, currentGame, onCurrentGameIdChanged, selectedGameId, onSelectedGameIdChanged, disabled }: GameToolbarProps) => {

    const { translate } = useI18n();
    const { isMobile } = useSidebar();
    const [showGameSelect, setShowGameSelect] = useState(true);

    return (
        <div className="flex flex-wrap flex-col justify-center md:flex-row md:flex-nowrap w-full gap-2.5 p-2">
            { isMobile &&
                <div className="flex grow justify-between">
                    <MobileSidebarTrigger />
                    <div className="flex grow justify-end gap-2">
                        <UserMenu disabled={disabled} />
                        <ViewMenu disabled={disabled} />
                        <SettingsMenu disabled={disabled} />
                        <Button variant="ghost" className="inline" disabled={disabled} onClick={() => setShowGameSelect(v => !v)}>
                            { showGameSelect ? <ChevronUp /> : <ChevronDown /> }
                        </Button>
                    </div>
                </div>
            }
            { (!isMobile || showGameSelect) &&
                <>
                    <div className={cn("flex flex-wrap w-full justify-center bg-card gap-2", isMobile && "flex-nowrap")}>
                        <div className="w-full xl:w-1/2 overflow-hidden">
                            <GameSelectMenu 
                                games={games} 
                                currentGame={currentGame} 
                                selectedGameId={selectedGameId} 
                                onSelectedGameIdChanged={onSelectedGameIdChanged} 
                                disabled={disabled}
                            />
                        </div>
                        <div className="flex gap-2">
                            <ConfirmMakeCurrentDialog onAccept={() => selectedGameId && onCurrentGameIdChanged(selectedGameId)}>
                                <Button variant="secondary" disabled={disabled || selectedGameId === currentGame?.id}>
                                    <Repeat />
                                    <span className="hidden sm:inline">{translate("GameToolbar.MakeCurrent")}</span>
                                </Button>
                            </ConfirmMakeCurrentDialog>
                            <NewGameDialogTrigger>
                                <Button variant="creative" disabled={disabled}>
                                    <SquarePlus />
                                    <span className="hidden sm:inline">{translate("GameToolbar.NewGame")}</span>
                                </Button>
                            </NewGameDialogTrigger>
                        </div>
                    </div>
                    { !isMobile &&
                        <div className="flex justify-end gap-2">
                            <UserMenu disabled={disabled} />
                            <ViewMenu disabled={disabled} />
                            <SettingsMenu disabled={disabled} />
                        </div>
                    }
                </>
            }
        </div>
    )
}