import { Separator } from "@/components/ui/separator";
import { GameToolbar } from "./components/GameToolbar";
import { useCallback, useEffect, useState } from "react";
import { GameStateContextProvider, useCurrentGame, useGamesList } from "@/hooks";
import { useSearchParams } from "react-router-dom";
import { ControlPanel } from "./components/ControlPanel";
import { NewGameDialog, NewGameDialogContainer } from "./components/NewGameDialog";
import { useGameApi } from "@/hooks/GameApiHook";

export const ScoreboardControl = () => {
    const games = useGamesList();
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { currentGame, setCurrentGame } = useCurrentGame();
    const [selectedGameId, setSelectedGameId] = useState<string | undefined>(searchParams.get('gameId') ?? '');
    const { createGame } = useGameApi();

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

    const handleNewGameCreated = async (homeTeamId: string, awayTeamId: string, gameName: string) => {
        const gameId = await createGame(gameName);

        setSelectedGameId(gameId);
    }

    return (
        <>
            <NewGameDialogContainer>
                <GameToolbar 
                    games={games} 
                    currentGame={currentGame} 
                    onCurrentGameIdChanged={setCurrentGame} 
                    selectedGameId={selectedGameId} 
                    onSelectedGameIdChanged={updateSelectedGameId} 
                />
                <Separator />
                <div className="px-5">
                    <GameStateContextProvider gameId={selectedGameId}>
                        <ControlPanel 
                            gameId={selectedGameId}
                        />
                    </GameStateContextProvider>
                    <NewGameDialog onNewGameCreated={handleNewGameCreated} />
                </div>
            </NewGameDialogContainer>
        </>
    );
}