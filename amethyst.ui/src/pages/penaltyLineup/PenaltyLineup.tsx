import { useCallback, useState } from "react";
import { useSearchParams } from "react-router-dom";

import { Separator } from "@/components/ui";
import { GameStateContextProvider, useCurrentGame, useGamesList, useI18n } from "@/hooks";

import { GameToolbar, Lineup } from "./components";

export const PenaltyLineup = () => {

    const { translate } = useI18n({ prefix: "PenaltyLineup." });
    const games = useGamesList();
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { currentGame } = useCurrentGame();

    const [selectedGameId, setSelectedGameId] = useState<string | undefined>(searchParams.get('gameId') ?? '');
    
    const updateSelectedGameId = useCallback((gameId?: string) => {
        searchParams.set('gameId', gameId ?? '');
        setSearchParams(searchParams);
        setSelectedGameId(gameId);
    }, [setSelectedGameId]);

    return (
        <>
            <title>{translate("Title")} | {translate("Main.Title", { ignorePrefix: true })}</title>
            <GameStateContextProvider gameId={selectedGameId}>
                <GameToolbar 
                    games={games} 
                    currentGame={currentGame} 
                    selectedGameId={selectedGameId} 
                    onSelectedGameIdChanged={updateSelectedGameId}
                />
                <Separator />
                <div className="flex flex-col p-1 md:p-2 xl:p-5 gap-1 md:gap-2 xl:gap-5">
                    <Lineup />
                    <div className="flex gap-1 pt-2">
                    </div>
                </div>
            </GameStateContextProvider>
        </>
    );
}