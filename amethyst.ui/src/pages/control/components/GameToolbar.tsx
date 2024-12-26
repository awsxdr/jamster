import { Button } from "@/components/ui/button"
import { GameSelectMenu } from "./GameSelectMenu"
import { ChevronDown, ChevronUp, Repeat, Settings, SquarePlus } from "lucide-react"
import { ConfirmMakeCurrentDialog } from "./ConfirmMakeCurrentDialog";
import { GameInfo } from "@/types";
import { useI18n } from "@/hooks/I18nHook";
import { NewGameDialogTrigger } from "../../../components/NewGameDialog";
import { ViewMenu } from "./ViewMenu";
import { MobileSidebarTrigger } from "@/components/MobileSidebarTrigger";
import { useSidebar } from "@/components/ui";
import { useState } from "react";
import { UserMenu } from "./UserMenu";
import { SettingsMenu } from "./SettingsMenu";

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
        <div className="flex flex-wrap flex-col justify-center md:flex-nowrap md:flex-row w-full gap-2.5 p-2">
            <div className="flex grow justify-between">
                <MobileSidebarTrigger />
                { isMobile &&
                    <div className="flex grow justify-end gap-2">
                        <UserMenu disabled={disabled} />
                        <ViewMenu disabled={disabled} />
                        <Button size="icon" variant="ghost" disabled={disabled}>
                            <Settings />
                        </Button>
                        <Button variant="ghost" className="inline" disabled={disabled} onClick={() => setShowGameSelect(v => !v)}>
                            { showGameSelect ? <ChevronUp /> : <ChevronDown /> }
                        </Button>
                    </div>
                }
            </div>
            { (!isMobile || showGameSelect) &&
                <>
                    <div className="flex w-full md:w-auto">
                        <GameSelectMenu 
                            games={games} 
                            currentGame={currentGame} 
                            selectedGameId={selectedGameId} 
                            onSelectedGameIdChanged={onSelectedGameIdChanged} 
                            disabled={disabled}
                        />
                    </div>
                    <div className="flex grow">
                        <div className="flex flex-wrap bg-card gap-2">
                            <ConfirmMakeCurrentDialog onAccept={() => selectedGameId && onCurrentGameIdChanged(selectedGameId)}>
                                <Button variant="secondary" disabled={disabled || selectedGameId === currentGame?.id}>
                                    <Repeat />
                                    <span className="hidden lg:inline">{translate("GameToolbar.MakeCurrent")}</span>
                                </Button>
                            </ConfirmMakeCurrentDialog>
                            <NewGameDialogTrigger>
                                <Button variant="creative" disabled={disabled}>
                                    <SquarePlus />
                                    <span className="hidden lg:inline">{translate("GameToolbar.NewGame")}</span>
                                </Button>
                            </NewGameDialogTrigger>
                        </div>
                        { !isMobile &&
                            <div className="flex grow justify-end gap-2">
                                <UserMenu disabled={disabled} />
                                <ViewMenu disabled={disabled} />
                                <SettingsMenu disabled={disabled} />
                            </div>
                        }
                    </div>
                </>
            }
        </div>
    )
}