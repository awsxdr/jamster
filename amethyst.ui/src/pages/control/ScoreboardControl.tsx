import { Separator } from "@/components/ui/separator";
import { GameToolbar } from "./components/GameToolbar";
import { useCallback, useEffect, useState } from "react";
import { GameStateContextProvider, useCreateGame, useCurrentGame, useGamesList, useI18n } from "@/hooks";
import { useSearchParams } from "react-router-dom";
import { ControlPanel } from "./components/ControlPanel";
import { NewGameCreated, NewGameDialog, NewGameDialogContainer } from "../../components/NewGameDialog";

export const ScoreboardControl = () => {
    const games = useGamesList();
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { currentGame, setCurrentGame } = useCurrentGame();
    const createGame = useCreateGame();
    const { translate }=  useI18n();

    const [selectedGameId, setSelectedGameId] = useState<string | undefined>(searchParams.get('gameId') ?? '');
    const [newGameDialogOpen, setNewGameDialogOpen] = useState(false);

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

    const handleNewGameCreated: NewGameCreated = async (...parameters) => {
        const gameId = await createGame(...parameters);

        setSelectedGameId(gameId);

        setNewGameDialogOpen(false);
    }

    const handleNewGameCancelled = () => {
        setNewGameDialogOpen(false);
    }

    return (
        <>
            <title>{translate("ScoreboardControl.Title")} | {translate("Main.Title")}</title>
            <NewGameDialogContainer open={newGameDialogOpen} onOpenChange={setNewGameDialogOpen}>
                <GameToolbar 
                    games={games} 
                    currentGame={currentGame} 
                    onCurrentGameIdChanged={setCurrentGame} 
                    selectedGameId={selectedGameId} 
                    onSelectedGameIdChanged={updateSelectedGameId}
                />
                <Separator />
                <div className="px-0 sm:px-1 md:px-2 xl:px-5">
                    <GameStateContextProvider gameId={selectedGameId}>
                        <ControlPanel 
                            gameId={selectedGameId}
                        />
                    </GameStateContextProvider>
                    <NewGameDialog 
                        onNewGameCreated={handleNewGameCreated}
                        onCancelled={handleNewGameCancelled}
                    />
                </div>
            </NewGameDialogContainer>
        </>
    );
}