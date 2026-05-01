import { useSearchParams } from "react-router-dom";
import { GameStateContextProvider, I18nContextProvider, useGameIdFromQueryString, useI18n, useQueryStringConfiguration } from "@/hooks"
import { useEffect, useState } from "react";
import { ActivityData, ClientActivity, DisplaySide, TeamSide } from "@/types";
import { ConnectionLostAlert } from "@/components/ConnectionLostAlert";
import { PenaltyTable } from "./components";
import languages from "@/i18n";

export const PenaltyWhiteboard = () => (
    <I18nContextProvider usageKey="whiteboard" defaultLanguage='en' languages={languages}>
        <PenaltyWhiteboardInner />
    </I18nContextProvider>
)

const PenaltyWhiteboardInner = () => {
    const { translate, setLanguage } = useI18n({ prefix: "PenaltyWhiteboard." });
    const [ searchParams, setSearchParams ] = useSearchParams();

    const [team, setTeam] = useState(DisplaySide.Both);

    const { languageCode } = useQueryStringConfiguration<ActivityData>({
        activity: () => ClientActivity.PenaltyWhiteboard,
        gameId: v => v,
        languageCode: v => v,
    }, {
        activity: ClientActivity.PenaltyWhiteboard,
        gameId: "",
        languageCode: "en",
    },
    ["activity"]);

    useEffect(() => {
        setLanguage(languageCode);
    }, [languageCode]);

    useEffect(() => {
        const searchParamsTeam = DisplaySide[searchParams.get('team') as keyof typeof DisplaySide];

        if(searchParamsTeam) {
            setTeam(searchParamsTeam);
        } else {
            searchParams.set('team', team);
            setSearchParams(searchParams, { replace: true });
        }
    }, [searchParams]);

    const gameId = useGameIdFromQueryString();

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