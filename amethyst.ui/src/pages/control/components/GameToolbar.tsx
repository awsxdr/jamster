import { Button } from "@/components/ui/button"
import { GameSelectMenu } from "./GameSelectMenu"
import { Repeat, Settings, SquarePlus } from "lucide-react"
import { ConfirmMakeCurrentDialog } from "./ConfirmMakeCurrentDialog";
import { GameInfo } from "@/types";
import { useI18n } from "@/hooks/I18nHook";
import { NewGameDialogTrigger } from "./NewGameDialog";
import { ViewMenu } from "./ViewMenu";

type GameToolbarProps = {
    games: GameInfo[];
    currentGame?: GameInfo;
    onCurrentGameIdChanged: (gameId: string) => void;
    selectedGameId?: string;
    onSelectedGameIdChanged: (gameId: string) => void;
}

export const GameToolbar = ({ games, currentGame, onCurrentGameIdChanged, selectedGameId, onSelectedGameIdChanged }: GameToolbarProps) => {

    const { translate } = useI18n();

    return (
        <div className="flex w-full p-5 pb-2">
            <div className="flex grow justify-center bg-card gap-2">
                <GameSelectMenu 
                    games={games} 
                    currentGame={currentGame} 
                    selectedGameId={selectedGameId} 
                    onSelectedGameIdChanged={onSelectedGameIdChanged} 
                />
                <ConfirmMakeCurrentDialog onAccept={() => selectedGameId && onCurrentGameIdChanged(selectedGameId)}>
                    <Button variant="secondary" disabled={selectedGameId === currentGame?.id}>
                        <Repeat />
                        <span className="hidden lg:inline">{translate("GameToolbar.MakeCurrent")}</span>
                    </Button>
                </ConfirmMakeCurrentDialog>
                <NewGameDialogTrigger>
                    <Button variant="creative">
                        <SquarePlus />
                        <span className="hidden lg:inline">{translate("GameToolbar.NewGame")}</span>
                    </Button>
                </NewGameDialogTrigger>
            </div>
            <div className="flex justify-center gap-2">
                <ViewMenu />
                <Button size="icon" variant="ghost">
                    <Settings />
                </Button>
            </div>
        </div>
    )
}