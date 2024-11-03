import { Button } from "@/components/ui/button"
import { GameSelectMenu } from "./GameSelectMenu"
import { Repeat, SquarePlus } from "lucide-react"
import styles from './GameToolbar.module.scss';
import { ConfirmMakeCurrentDialog } from "./ConfirmMakeCurrentDialog";
import { GameInfo } from "@/types";

type GameToolbarProps = {
    games: GameInfo[];
    currentGame?: GameInfo;
    onCurrentGameIdChanged: (gameId: string) => void;
    selectedGameId?: string;
    onSelectedGameIdChanged: (gameId: string) => void;
}

export const GameToolbar = ({ games, currentGame, onCurrentGameIdChanged, selectedGameId, onSelectedGameIdChanged }: GameToolbarProps) => {

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
                    <span className="hidden lg:inline">Make current</span>
                </Button>
            </ConfirmMakeCurrentDialog>
            <Button variant="creative">
                <SquarePlus />
                <span className="hidden lg:inline">New game...</span>
            </Button>
        </div>
    )
}