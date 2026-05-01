import { Separator } from "@/components/ui/separator";
import { GameToolbar } from "./components/GameToolbar";
import { useCallback, useEffect, useState } from "react";
import { GameStateContextProvider, useCreateGame, useCurrentGame, useCurrentUserConfiguration, useGamesList, useI18n } from "@/hooks";
import { useSearchParams } from "react-router-dom";
import { ControlPanel } from "./components/ControlPanel";
import { NewGameCreated, NewGameDialog, NewGameDialogContainer } from "../../components/NewGameDialog";
import { Timeline } from "./components/timeline";
import { ControlPanelViewConfiguration, DEFAULT_CONTROL_PANEL_VIEW_CONFIGURATION } from "@/types";

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
        setSearchParams(searchParams, { replace: true });
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

    const { configuration: viewConfiguration } = useCurrentUserConfiguration<ControlPanelViewConfiguration>("ControlPanelViewConfiguration", DEFAULT_CONTROL_PANEL_VIEW_CONFIGURATION);

    return (
        <>
            <title>{translate("ScoreboardControl.Title")} | {translate("Main.Title")}</title>
            <NewGameDialogContainer open={newGameDialogOpen} onOpenChange={setNewGameDialogOpen}>
                <div className="w-full h-full max-h-[100vh] overflow-hidden flex flex-col">
                    <GameToolbar 
                        games={games} 
                        currentGame={currentGame} 
                        onCurrentGameIdChanged={setCurrentGame} 
                        selectedGameId={selectedGameId} 
                        onSelectedGameIdChanged={updateSelectedGameId}
                    />
                    { selectedGameId && (
                        <GameStateContextProvider gameId={selectedGameId}>
                            <Separator />
                            <div className="px-0 sm:px-1 md:px-2 xl:px-5 overflow-y-auto">
                                <ControlPanel 
                                    gameId={selectedGameId}
                                    viewConfiguration={viewConfiguration}
                                />
                                <NewGameDialog 
                                    onNewGameCreated={handleNewGameCreated}
                                    onCancelled={handleNewGameCancelled}
                                />
                            </div>
                            <Separator />
                            { viewConfiguration.showTimeline && (
                                <Timeline gameId={selectedGameId} />
                            )}
                        </GameStateContextProvider>
                    )}
                </div>
            </NewGameDialogContainer>
        </>
    );
}