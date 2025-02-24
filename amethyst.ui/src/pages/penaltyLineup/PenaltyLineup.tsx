import { useCallback, useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";

import { Separator } from "@/components/ui";
import { GameStateContextProvider, useCurrentGame, useGamesList, useI18n } from "@/hooks";

import { GameToolbar } from "./components";
import { PenaltyLineupTable } from "./components/PenaltyLineupTable";
import { DisplaySide, TeamSide } from "@/types";

export const PenaltyLineup = () => {

    const { translate } = useI18n({ prefix: "PenaltyLineup." });
    const games = useGamesList();
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { currentGame } = useCurrentGame();

    const [selectedGameId, setSelectedGameId] = useState<string | undefined>(searchParams.get('gameId') ?? '');
    const [team, setTeam] = useState(DisplaySide.Home);
    
    const updateSelectedGameId = useCallback((gameId?: string) => {
        searchParams.set('gameId', gameId ?? '');
        setSearchParams(searchParams);
        setSelectedGameId(gameId);
    }, [setSelectedGameId]);

    useEffect(() => {
        if (!selectedGameId && currentGame) {
            updateSelectedGameId(currentGame.id);
        }
    }, [currentGame, selectedGameId]);

    useEffect(() => {
        const searchParamsTeam = searchParams.get('team') as keyof typeof DisplaySide;

        if(searchParamsTeam) {
            setTeam(DisplaySide[searchParamsTeam]);
        } else {
            searchParams.set('team', team);
            setSearchParams(searchParams);
        }
    }, [searchParams]);

    const handleTeamChanged = (displaySide: DisplaySide) => {
        searchParams.set('team', displaySide);
        setSearchParams(searchParams);
    }

    return (
        <>
            <title>{translate("Title")} | {translate("Main.Title", { ignorePrefix: true })}</title>
            <GameStateContextProvider gameId={selectedGameId}>
                <GameToolbar 
                    games={games} 
                    currentGame={currentGame} 
                    selectedGameId={selectedGameId} 
                    displaySide={team}
                    onDisplaySideChanged={handleTeamChanged}
                    onSelectedGameIdChanged={updateSelectedGameId}
                />
                <Separator />
                { selectedGameId && (
                    <div className="flex flex-col 2xl:flex-row">
                        { team !== DisplaySide.Away && (
                            <div className="flex flex-col p-1 md:p-2 xl:p-5 gap-1 md:gap-2 xl:gap-5">
                                <PenaltyLineupTable teamSide={TeamSide.Home} gameId={selectedGameId} compact={team === DisplaySide.Both} />
                            </div>
                        )}
                        { team !== DisplaySide.Home && (
                            <div className="flex flex-col p-1 md:p-2 xl:p-5 gap-1 md:gap-2 xl:gap-5">
                                <PenaltyLineupTable teamSide={TeamSide.Away} gameId={selectedGameId} compact={team === DisplaySide.Both} />
                            </div>
                        )}
                    </div>
                )}
            </GameStateContextProvider>
        </>
    );
}