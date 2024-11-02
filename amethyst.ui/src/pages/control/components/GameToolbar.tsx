import { Button } from "@/components/ui/button"
import { GameSelectMenu } from "./GameSelectMenu"
import { Repeat, SquarePlus } from "lucide-react"
import { useEffect, useState } from "react";
import { useCurrentGame, useGamesList } from "@/hooks";
import styles from './GameToolbar.module.scss';
import { ConfirmMakeCurrentDialog } from "./ConfirmMakeCurrentDialog";

export const GameToolbar = () => {

    const games = useGamesList();
    const { currentGame, setCurrentGame } = useCurrentGame();
    const [selectedGameId, setSelectedGameId] = useState<string | undefined>(currentGame?.id);
    
    useEffect(() => {
        if (!selectedGameId) {
            setSelectedGameId(currentGame?.id);
        }
    }, [currentGame]);
    
    return (
        <div className={styles.toolbar}>
            <GameSelectMenu 
                games={games} 
                currentGame={currentGame} 
                selectedGameId={selectedGameId} 
                onSelectedGameIdChanged={setSelectedGameId} 
            />
            <ConfirmMakeCurrentDialog onAccept={() => selectedGameId && setCurrentGame(selectedGameId)}>
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