import { GameStateContextProvider, useCurrentGame, useGameStageState } from '@/hooks';
import { Stage, TeamSide } from '@/types';
import { TeamDetails } from './components/TeamDetails';
import { JamDetails } from './components/JamDetails';
import { TimeoutDetails } from './components/TimeoutDetails';
import { LineupDetails } from './components/LineupDetails';
import { TeamColorGradients } from './components/TeamColorGradients';
import { IntermissionDetails } from './components/IntermissionDetails';
import { useEffect, useState } from 'react';
import { Button } from '@/components/ui';
import { Maximize2 } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useWakeLock } from '@/hooks/WakeLock';

export const CurrentGameScoreboard = () => {
    const { currentGame } = useCurrentGame();
  
    return (
      <GameStateContextProvider gameId={currentGame?.id}>
        <Scoreboard />
      </GameStateContextProvider>
    );
};

export const Scoreboard = () => {

    const { translate } = useI18n();

    const gameStage = useGameStageState() ?? { stage: Stage.BeforeGame, periodNumber: 0, jamNumber: 0, periodIsFinalized: false };

    const [fullScreenButtonVisible, setFullScreenButtonVisible] = useState(false);
    const [userInteracting, setUserInteracting] = useState(0);

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
            <TeamColorGradients />
            <div className="absolute left-0 top-0 h-full w-full select-none overflow-hidden px-2" onMouseMove={handleUserInteracting} onTouchStart={handleUserInteracting}>
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
                        <div className="h-screen flex flex-col justify-center gap-5">
                            <div className="flex justify-around items-stretch h-[45vh] gap-5">
                                <TeamDetails side={TeamSide.Home} />
                                <TeamDetails side={TeamSide.Away} />
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
        </>
    );
}