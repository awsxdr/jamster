import { JamClock, PeriodClock } from '@components/clocks';
import { ScoreboardComponent } from '@/pages/scoreboard/components/ScoreboardComponent';
import { GameStateContextProvider, useCurrentGame, useGameStageState } from '@/hooks';
import { Stage, TeamSide } from '@/types';
import { TeamDetails } from './components/TeamDetails';

export const CurrentGameScoreboard = () => {
    const { currentGame } = useCurrentGame();
  
    return (
      <GameStateContextProvider gameId={currentGame?.id}>
        <Scoreboard />
      </GameStateContextProvider>
    );
};

export const Scoreboard = () => {

    const gameStage = useGameStageState() ?? { stage: Stage.BeforeGame, periodNumber: 0, jamNumber: 0 };

    return (
        <>
            <div className="flex bg-black w-full h-full">
                <div className="inline-block grow"></div>
                <div className="inline-flex flex-col mw-[140vh] w-full">
                    <div className="h-screen flex flex-col justify-center">
                        <div className="flex justify-around items-stretch h-[50vh] overflow-hidden">
                            <TeamDetails side={TeamSide.Home} />
                            <TeamDetails side={TeamSide.Away} />
                        </div>
                        <div className="flex justify-around p-4 h-[30vh]">
                            <ScoreboardComponent className="w-2/5 h-full" header={`Period ${gameStage.periodNumber}`}>
                                <PeriodClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" />
                            </ScoreboardComponent>
                            <ScoreboardComponent className="w-2/5 h-full" header={`Jam ${gameStage.jamNumber}`}>
                                <JamClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" />
                            </ScoreboardComponent>
                        </div>
                    </div>
                </div>
                <div className="inline-block grow"></div>
            </div>
        </>
    );
}