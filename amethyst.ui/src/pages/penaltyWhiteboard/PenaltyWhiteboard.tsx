import { useSearchParams } from "react-router-dom";
import { GameStateContextProvider, I18nContextProvider, useCurrentGame, useI18n } from "@/hooks"
import { useEffect, useMemo, useState } from "react";
import { DisplaySide, TeamSide } from "@/types";
import { ConnectionLostAlert } from "@/components/ConnectionLostAlert";
import { PenaltyTable } from "./components";
import languages from "@/i18n";

export const PenaltyWhiteboard = () => (
    <I18nContextProvider usageKey="whiteboard" defaultLanguage='en' languages={languages}>
        <PenaltyWhiteboardInner />
    </I18nContextProvider>
)

const PenaltyWhiteboardInner = () => {
    const { translate } = useI18n({ prefix: "PenaltyWhiteboard." });
    const [ searchParams, setSearchParams ] = useSearchParams();
    const { currentGame } = useCurrentGame();

    const [selectedGameId, setSelectedGameId] = useState<string | undefined>(searchParams.get('gameId') ?? '');
    const [team, setTeam] = useState(DisplaySide.Both);

    const gameId = useMemo(() => selectedGameId === "current" ? currentGame?.id : selectedGameId, [selectedGameId, currentGame]);

    useEffect(() => {
        if (!selectedGameId && currentGame) {
            searchParams.set('gameId', "current");
            setSearchParams(searchParams);
            setSelectedGameId(currentGame.id);
        }
    }, [currentGame, selectedGameId]);

    useEffect(() => {
        const searchParamsTeam = DisplaySide[searchParams.get('team') as keyof typeof DisplaySide];

        if(searchParamsTeam) {
            setTeam(searchParamsTeam);
        } else {
            searchParams.set('team', team);
            setSearchParams(searchParams);
        }
    }, [searchParams]);

    return (
        <>
            <title>{translate("Title")} | {translate("Main.Title", { ignorePrefix: true })}</title>
            { gameId && (
                <GameStateContextProvider gameId={gameId}>
                    <div className="flex flex-col p-5 gap-2 w-full">
                        <ConnectionLostAlert />
                        <div className="flex flex-col lg:flex-row gap-5 w-full">
                            { team !== DisplaySide.Away && (
                                <div className="w-full">
                                    <PenaltyTable teamSide={TeamSide.Home} />
                                </div>
                            )}
                            { team !== DisplaySide.Home && (
                                <div className="w-full">
                                    <PenaltyTable teamSide={TeamSide.Away} />
                                </div>
                            )}
                        </div>
                    </div>
                </GameStateContextProvider>
            )}
        </>
    )
}