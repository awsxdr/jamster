import { useCallback, useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";

import { Separator } from "@/components/ui";
import { GameStateContextProvider, useCurrentGame, useI18n } from "@/hooks";
import { BoxDisplayType, BoxTripList, GameToolbar, PenaltyLineupTable, PltDisplayType } from "./components";
import { DisplaySide, TeamSide } from "@/types";
import { ConnectionLostAlert } from "@/components/ConnectionLostAlert";

export const PenaltyLineup = () => {

    const { translate } = useI18n({ prefix: "PenaltyLineup." });
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { currentGame } = useCurrentGame();

    const [selectedGameId, setSelectedGameId] = useState<string | undefined>(searchParams.get('gameId') ?? '');
    const [team, setTeam] = useState(DisplaySide.Home);
    const [pltDisplay, setPltDisplay] = useState<PltDisplayType>("Both");
    const [boxDisplay, setBoxDisplay] = useState<BoxDisplayType>("None");
    
    const updateSelectedGameId = useCallback((gameId?: string) => {
        searchParams.set('gameId', gameId ?? '');
        setSearchParams(searchParams, { replace: true });
        setSelectedGameId(gameId);
    }, [setSelectedGameId]);

    useEffect(() => {
        if (!selectedGameId && currentGame) {
            updateSelectedGameId(currentGame.id);
        }
    }, [currentGame, selectedGameId]);

    useEffect(() => {
        const searchParamsTeam = DisplaySide[searchParams.get('team') as keyof typeof DisplaySide];

        if(searchParamsTeam) {
            setTeam(searchParamsTeam);
        } else {
            searchParams.set('team', team);
            setSearchParams(searchParams, { replace: true });
        }
    }, [searchParams]);

    useEffect(() => {
        const searchParamsPltType = searchParams.get('plt') as PltDisplayType;
        if(!["Both", "Penalties", "Lineup", "None"].includes(searchParamsPltType)) {
            searchParams.set('plt', "Both");
            setSearchParams(searchParams, { replace: true });
            return;
        }

        setPltDisplay(searchParamsPltType);
    }, [searchParams]);

    useEffect(() => {
        const searchParamsBoxType = searchParams.get('box') as BoxDisplayType;
        if(!["Both", "None", "Jammers", "Blockers"].includes(searchParamsBoxType)) {
            searchParams.set("box", "None");
            setSearchParams(searchParams, { replace: true });
            return;
        }

        setBoxDisplay(searchParamsBoxType);
    }, [searchParams]);

    const handleTeamChanged = (displaySide: DisplaySide) => {
        searchParams.set('team', displaySide);
        setSearchParams(searchParams, { replace: true });
    }

    const handlePltTypeChanged = (pltType: PltDisplayType) => {
        searchParams.set('plt', pltType);
        setSearchParams(searchParams, { replace: true });
    }

    const handleBoxTypeChanged = (boxType: BoxDisplayType) => {
        searchParams.set('box', boxType);
        setSearchParams(searchParams, { replace: true });
    }

    return (
        <>
            <title>{translate("Title")} | {translate("Main.Title", { ignorePrefix: true })}</title>
            <GameToolbar 
                    currentGame={currentGame} 
                    selectedGameId={selectedGameId} 
                    displaySide={team}
                    pltDisplayType={pltDisplay}
                    boxDisplayType={boxDisplay}
                    onDisplaySideChanged={handleTeamChanged}
                    onPltDisplayTypeChanged={handlePltTypeChanged}
                    onBoxDisplayTypeChanged={handleBoxTypeChanged}
                    onSelectedGameIdChanged={updateSelectedGameId}
                />
            { selectedGameId && (
                <GameStateContextProvider gameId={selectedGameId}>
                    <Separator />
                    <ConnectionLostAlert />
                    { pltDisplay !== "None" && (
                        <div className="flex flex-col 2xl:flex-row">
                            { team !== DisplaySide.Away && (
                                <div className="flex flex-col p-1 md:p-2 xl:p-5 gap-1 md:gap-2 xl:gap-5 w-full">
                                    <PenaltyLineupTable teamSide={TeamSide.Home} gameId={selectedGameId} compact={team === DisplaySide.Both && pltDisplay === "Both"} display={pltDisplay} />
                                </div>
                            )}
                            { team !== DisplaySide.Home && (
                                <div className="flex flex-col p-1 md:p-2 xl:p-5 gap-1 md:gap-2 xl:gap-5 w-full">
                                    <PenaltyLineupTable teamSide={TeamSide.Away} gameId={selectedGameId} compact={team === DisplaySide.Both && pltDisplay === "Both"} display={pltDisplay} />
                                </div>
                            )}
                        </div>
                    )}
                    <Separator />
                    { boxDisplay !== "None" && (
                        <BoxTripList teamSide={TeamSide.Home} boxDisplayType={boxDisplay} />
                    )}
                </GameStateContextProvider>
            )}
        </>
    );
}