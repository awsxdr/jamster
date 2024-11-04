import { Button } from "@/components/ui/button"
import { GameSelectMenu } from "./GameSelectMenu"
import { Repeat, SquarePlus } from "lucide-react"
import styles from './GameToolbar.module.scss';
import { ConfirmMakeCurrentDialog } from "./ConfirmMakeCurrentDialog";
import { GameInfo } from "@/types";
import { useI18n } from "@/hooks/I18nHook";

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
        <div className={styles.toolbar}>
            <GameSelectMenu 
                games={games} 
                currentGame={currentGame} 
                selectedGameId={selectedGameId} 
                onSelectedGameIdChanged={onSelectedGameIdChanged} 
            />
            <ConfirmMakeCurrentDialog onAccept={() => selectedGameId && onCurrentGameIdChanged(selectedGameId)}>
                <Button variant="secondary" disabled={selectedGameId === currentGame?.id}>
                    <Repeat />
                    <span className="hidden lg:inline">{translate("Make current")}</span>
                </Button>
            </ConfirmMakeCurrentDialog>
            <Button variant="creative">
                <SquarePlus />
                <span className="hidden lg:inline">{translate("New game...")}</span>
            </Button>
        </div>
    )
}