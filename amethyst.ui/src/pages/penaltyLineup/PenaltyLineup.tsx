import { useCallback, useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";

import { Separator } from "@/components/ui";
import { GameStateContextProvider, useCurrentGame, useI18n } from "@/hooks";

import { GameToolbar, PltDisplayType } from "./components";
import { PenaltyLineupTable } from "./components/PenaltyLineupTable";
import { DisplaySide, TeamSide } from "@/types";

export const PenaltyLineup = () => {

    const { translate } = useI18n({ prefix: "PenaltyLineup." });
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { currentGame } = useCurrentGame();

    const [selectedGameId, setSelectedGameId] = useState<string | undefined>(searchParams.get('gameId') ?? '');
    const [team, setTeam] = useState(DisplaySide.Home);
    const [display, setDisplay] = useState<PltDisplayType>("Both");
    
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

    useEffect(() => {
        const searchParamsPltType = searchParams.get('plt') as PltDisplayType;
        if(!["Both", "Penalties", "Lineup", "None", "", undefined].includes(searchParamsPltType)) {
            searchParams.set('plt', "Both");
            setSearchParams(searchParams);
            return;
        }

        if(searchParamsPltType) {
            setDisplay(searchParamsPltType);
        } else {
            searchParams.set('plt', display);
            setSearchParams(searchParams);
        }
    }, [searchParams]);

    const handleTeamChanged = (displaySide: DisplaySide) => {
        searchParams.set('team', displaySide);
        setSearchParams(searchParams);
    }

    const handlePltTypeChanged = (pltType: PltDisplayType) => {
        searchParams.set('plt', pltType);
        setSearchParams(searchParams);
    }

    return (
        <>
            <title>{translate("Title")} | {translate("Main.Title", { ignorePrefix: true })}</title>
            <GameToolbar 
                    currentGame={currentGame} 
                    selectedGameId={selectedGameId} 
                    displaySide={team}
                    pltDisplayType={display}
                    onDisplaySideChanged={handleTeamChanged}
                    onPltDisplayTypeChanged={handlePltTypeChanged}
                    onSelectedGameIdChanged={updateSelectedGameId}
                />
            { selectedGameId && (
                <GameStateContextProvider gameId={selectedGameId}>
                    <Separator />
                    { selectedGameId && display !== "None" && (
                        <div className="flex flex-col 2xl:flex-row">
                            { team !== DisplaySide.Away && (
                                <div className="flex flex-col p-1 md:p-2 xl:p-5 gap-1 md:gap-2 xl:gap-5 w-full">
                                    <PenaltyLineupTable teamSide={TeamSide.Home} gameId={selectedGameId} compact={team === DisplaySide.Both && display === "Both"} display={display} />
                                </div>
                            )}
                            { team !== DisplaySide.Home && (
                                <div className="flex flex-col p-1 md:p-2 xl:p-5 gap-1 md:gap-2 xl:gap-5 w-full">
                                    <PenaltyLineupTable teamSide={TeamSide.Away} gameId={selectedGameId} compact={team === DisplaySide.Both && display === "Both"} display={display} />
                                </div>
                            )}
                        </div>
                    )}
                </GameStateContextProvider>
            )}
        </>
    );
}