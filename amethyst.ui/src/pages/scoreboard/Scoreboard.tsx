import { GameStateContextProvider, I18nContextProvider, useGameIdFromQueryString, useGameStageState, useHasServerConnection, useI18n, useQueryStringConfiguration } from '@/hooks';
import { ActivityData, ClientActivity, DEFAULT_DISPLAY_CONFIGURATION, ScoreboardActivity, Stage, TeamSide } from '@/types';
import { TeamDetails } from './components/TeamDetails';
import { JamDetails } from './components/JamDetails';
import { TimeoutDetails } from './components/TimeoutDetails';
import { LineupDetails } from './components/LineupDetails';
import { TeamColorGradients } from './components/TeamColorGradients';
import { IntermissionDetails } from './components/IntermissionDetails';
import { PropsWithChildren, useEffect, useState } from 'react';
import { Button } from '@/components/ui';
import { Maximize2 } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useWakeLock } from '@/hooks/WakeLock';
import languages from '@/i18n';
import { ScaledText } from '@/components/ScaledText';

export const SCOREBOARD_GAP_CLASS_NAME = "gap-1 md:gap-2 lg:gap-3 xl:gap-5";
export const SCOREBOARD_TEXT_PADDING_CLASS_NAME = "p-1 md:p-2 lg:p-3 xl:p-5";

type ScoreboardContextProps = {
    gameId: string;
}

const ScoreboardContext = ({ gameId, children }: PropsWithChildren<ScoreboardContextProps>) => (
    <I18nContextProvider usageKey="scoreboard" defaultLanguage='en' languages={languages}>
        <GameStateContextProvider gameId={gameId}>
            { children }
        </GameStateContextProvider>
    </I18nContextProvider>
);

const ScoreboardContent = () => {
    const { translate, setLanguage } = useI18n();

    const hasConnection = useHasServerConnection();
    const gameStage = useGameStageState() ?? { stage: Stage.BeforeGame, periodNumber: 0, jamNumber: 0, totalJamNumber: 0, periodIsFinalized: false };

    const [fullScreenButtonVisible, setFullScreenButtonVisible] = useState(false);
    const [userInteracting, setUserInteracting] = useState(0);

    const { languageCode, useSidebars, useNameBackgrounds } = useQueryStringConfiguration<ActivityData & ScoreboardActivity>({
            activity: () => ClientActivity.Scoreboard,
            gameId: v => v,
            languageCode: v => v,
            useSidebars: v => { 
                return v === "true";
            },
            useNameBackgrounds: v => v === "true",
        }, {
            activity: ClientActivity.Scoreboard,
            gameId: "",
            languageCode: DEFAULT_DISPLAY_CONFIGURATION.language,
            useSidebars: "true",
            useNameBackgrounds: "true",
        },
        ["activity"]
    );

    useEffect(() => {
        setLanguage(languageCode);
    }, [languageCode]);

    useEffect(() => {
        setFullScreenButtonVisible(true);

        const timeout = setTimeout(() => {
            setFullScreenButtonVisible(false);
        }, 2000);

        return () => {
            clearTimeout(timeout);
        }
    }, [userInteracting]);

    const handleUserInteracting = () => {
        setUserInteracting(c => c + 1);
    }

    const { acquireWakeLock, releaseWakeLock } = useWakeLock();

    const handleFullScreen = () => {
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen();
            acquireWakeLock();
        } else if (document.exitFullscreen) {
            document.exitFullscreen();
            releaseWakeLock();
        }
    }

    return (
        <>
            <title>{translate("Scoreboard.Title")} | {translate("Main.Title")}</title>
            <TeamColorGradients hidden={!useSidebars} />
            <div 
                className={cn(
                    "absolute left-0 top-0 h-full w-full select-none overflow-hidden", 
                    "px-0 md:px-1 lg:px-2"
                )} 
                onMouseMove={handleUserInteracting} 
                onTouchStart={handleUserInteracting}
            >
                <Button 
                    size="icon" 
                    variant="ghost" 
                    onClick={handleFullScreen}
                    className={cn("opacity-0 transition-opacity absolute top-0 right-0 text-white", fullScreenButtonVisible && "opacity-80")}
                >
                    <Maximize2 />
                </Button>
                <div className="flex w-full h-full justify-center">
                    <div className="inline-flex flex-col max-w-[140vh] w-full">
                        <div className={cn("h-screen flex flex-col justify-center", SCOREBOARD_GAP_CLASS_NAME)}>
                            <div className={cn("flex justify-around items-stretch h-[45vh]", SCOREBOARD_GAP_CLASS_NAME)}>
                                <TeamDetails side={TeamSide.Home} showBackgrounds={useNameBackgrounds} />
                                <TeamDetails side={TeamSide.Away} showBackgrounds={useNameBackgrounds} />
                            </div>
                            <div className="flex flex-col h-[35vh] w-full relative">
                                <JamDetails gameStage={gameStage} visible={gameStage.stage === Stage.Jam} />
                                <TimeoutDetails gameStage={gameStage} visible={gameStage.stage === Stage.Timeout || gameStage.stage === Stage.AfterTimeout} />
                                <LineupDetails gameStage={gameStage} visible={gameStage.stage === Stage.Lineup} />
                                <IntermissionDetails gameStage={gameStage} visible={gameStage.stage === Stage.BeforeGame || gameStage.stage === Stage.Intermission || gameStage.stage === Stage.AfterGame} />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            { !hasConnection && (
                <div className="absolute left-0 top-0 h-full w-full select-none overflow-hidden bg-gradient-to-b from-[rgba(255,0,0,.5)]">
                    <div className="absolute left-0 top-4 w-full h-[10vh] flex justify-center items-center text-center">
                        <div className="bg-[#d00] text-white rounded-md sm:rounded-lg md:rounded-xl xl:rounded-3xl h-full w-1/2">
                            <ScaledText text={translate("Scoreboard.ConnectionLost")} className="h-full w-full flex flex-col justify-center" />
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}

export const Scoreboard = () => {

    const gameId = useGameIdFromQueryString();

    if(!gameId) {
        return (<></>);
    }

    return (
        <ScoreboardContext gameId={gameId}>
            <ScoreboardContent />
        </ScoreboardContext>
    );
}