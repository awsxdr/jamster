import { Separator } from "@/components/ui/separator";
import { GameToolbar } from "./components/GameToolbar";
import { useCallback, useEffect, useState } from "react";
import { GameStateContextProvider, useCurrentGame, useGamesList } from "@/hooks";
import { useSearchParams } from "react-router-dom";
import { ControlPanel } from "./components/ControlPanel";

export const ScoreboardControl = () => {
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
        <>
            <GameToolbar 
                games={games} 
                currentGame={currentGame} 
                onCurrentGameIdChanged={setCurrentGame} 
                selectedGameId={selectedGameId} 
                onSelectedGameIdChanged={updateSelectedGameId} 
            />
            <Separator />
            <GameStateContextProvider gameId={selectedGameId}>
                <ControlPanel 
                    gameId={selectedGameId}
                />
            </GameStateContextProvider>
        </>
    );
}