import { Separator } from "@/components/ui/separator";
import { GameToolbar } from "./components/GameToolbar";
import { useCallback, useEffect, useState } from "react";
import { GameStateContextProvider, useCurrentGame, useGamesList } from "@/hooks";
import { useSearchParams } from "react-router-dom";
import { ControlPanel } from "./components/ControlPanel";
import { NewGameDialog, NewGameDialogContainer } from "../../components/NewGameDialog";
import { useGameApi } from "@/hooks/GameApiHook";
import { useEvents } from "@/hooks/EventsApiHook";
import { TeamSet } from "@/types/events";
import { Team, TeamSide } from "@/types";
import { useTeamApi } from "@/hooks/TeamApiHook";
import { UserSettingsProvider } from "@/hooks/UserSettings";

export const ScoreboardControl = () => {
    const games = useGamesList();
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { currentGame, setCurrentGame } = useCurrentGame();
    const { createGame } = useGameApi();
    const { sendEvent } = useEvents();
    const { getTeam } = useTeamApi();

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

    const handleNewGameCreated = async (homeTeamId: string, homeTeamColorIndex: number, awayTeamId: string, awayTeamColorIndex: number, gameName: string) => {
        const gameId = await createGame(gameName);

        const homeTeam = await getTeam(homeTeamId)
        const awayTeam = await getTeam(awayTeamId);

        const getTeamColor = (team: Team, colorIndex: number) => {
            const colorKeys = Object.keys(team.colors);
            if(colorKeys.length === 0) {
                return {
                    shirtColor: '#000000',
                    complementaryColor: '#ffffff',
                };
            }

            if(colorIndex > colorKeys.length) {
                colorIndex = 0;
            }

            return team.colors[colorKeys[colorIndex]]!;
        }

        const homeTeamColor = getTeamColor(homeTeam, homeTeamColorIndex);
        const awayTeamColor = getTeamColor(awayTeam, awayTeamColorIndex);

        const homeGameTeam = {
            names: homeTeam.names,
            color: homeTeamColor,
            roster: homeTeam.roster.map(s => ({ ...s, isSkating: true })),
        };

        const awayGameTeam = {
            names: awayTeam.names,
            color: awayTeamColor,
            roster: awayTeam.roster.map(s => ({ ...s, isSkating: true })),
        };
        
        await sendEvent(gameId, new TeamSet(TeamSide.Home, homeGameTeam));
        await sendEvent(gameId, new TeamSet(TeamSide.Away, awayGameTeam));

        setSelectedGameId(gameId);

        setNewGameDialogOpen(false);
    }

    const handleNewGameCancelled = () => {
        setNewGameDialogOpen(false);
    }

    return (
        <>
            <UserSettingsProvider>
                <NewGameDialogContainer open={newGameDialogOpen} onOpenChange={setNewGameDialogOpen}>
                    <GameToolbar 
                        games={games} 
                        currentGame={currentGame} 
                        onCurrentGameIdChanged={setCurrentGame} 
                        selectedGameId={selectedGameId} 
                        onSelectedGameIdChanged={updateSelectedGameId}
                    />
                    <Separator />
                    <div className="px-2 md:px-5">
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
            </UserSettingsProvider>
        </>
    );
}