import { Button } from "@/components/ui/button"
import { GameSelectMenu } from "./GameSelectMenu"
import { Repeat, SquarePlus } from "lucide-react"
import { useCallback, useEffect, useState } from "react";
import { useCurrentGame, useGamesList } from "@/hooks";
import styles from './GameToolbar.module.scss';
import { ConfirmMakeCurrentDialog } from "./ConfirmMakeCurrentDialog";
import { useSearchParams } from "react-router-dom";

export const GameToolbar = () => {

    const games = useGamesList();
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { currentGame, setCurrentGame } = useCurrentGame();
    const [selectedGameId, setSelectedGameId] = useState<string | undefined>(searchParams.get('gameId') ?? '');
    
    useEffect(() => {
        const gameId = searchParams.get('gameId');

        if(gameId && gameId !== selectedGameId) {
            setSelectedGameId(gameId);
        }
    }, [searchParams, setSelectedGameId])

    useEffect(() => {
        if (!selectedGameId) {
            updateSelectedGameId(currentGame?.id);
        }
    }, [currentGame, selectedGameId]);

    const updateSelectedGameId = useCallback((gameId?: string) => {
        searchParams.set('gameId', gameId ?? '');
        setSearchParams(searchParams);
        setSelectedGameId(gameId);
    }, [setSelectedGameId]);
    
    return (
        <div className={styles.toolbar}>
            <GameSelectMenu 
                games={games} 
                currentGame={currentGame} 
                selectedGameId={selectedGameId} 
                onSelectedGameIdChanged={updateSelectedGameId} 
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