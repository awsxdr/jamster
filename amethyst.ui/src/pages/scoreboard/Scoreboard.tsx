import { GameStateContextProvider, useCurrentGame, useGameStageState } from '@/hooks';
import { Stage, TeamSide } from '@/types';
import { TeamDetails } from './components/TeamDetails';
import { JamDetails } from './components/JamDetails';
import { TimeoutDetails } from './components/TimeoutDetails';
import { LineupDetails } from './components/LineupDetails';
import { TeamColorGradients } from './components/TeamColorGradients';
import { IntermissionDetails } from './components/IntermissionDetails';

export const CurrentGameScoreboard = () => {
    const { currentGame } = useCurrentGame();
  
    return (
      <GameStateContextProvider gameId={currentGame?.id}>
        <Scoreboard />
      </GameStateContextProvider>
    );
};

export const Scoreboard = () => {

    const gameStage = useGameStageState() ?? { stage: Stage.BeforeGame, periodNumber: 0, jamNumber: 0, periodIsFinalized: false };

    return (
        <>
            <TeamColorGradients />
            <div className="absolute left-0 top-0 h-full w-full">
                <div className="flex w-full h-full justify-center">
                    <div className="inline-flex flex-col max-w-[140vh] w-full">
                        <div className="h-screen flex flex-col justify-center">
                            <div className="flex justify-around items-stretch h-[50vh] gap-5">
                                <TeamDetails side={TeamSide.Home} />
                                <TeamDetails side={TeamSide.Away} />
                            </div>
                            <JamDetails gameStage={gameStage} visible={gameStage.stage === Stage.Jam} />
                            <TimeoutDetails gameStage={gameStage} visible={gameStage.stage === Stage.Timeout || gameStage.stage === Stage.AfterTimeout} />
                            <LineupDetails gameStage={gameStage} visible={gameStage.stage === Stage.Lineup} />
                            <IntermissionDetails gameStage={gameStage} visible={gameStage.stage === Stage.BeforeGame || gameStage.stage === Stage.Intermission || gameStage.stage === Stage.AfterGame} />
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
}