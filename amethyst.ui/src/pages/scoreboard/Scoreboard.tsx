import { JamClock, PeriodClock } from '@components/clocks';
import { ScoreboardComponent } from '@/pages/scoreboard/components/ScoreboardComponent';
import { GameStateContextProvider, useGameState, useCurrentGame } from '@/hooks';
import { GameStageState, Stage, TeamSide } from '@/types';
import { TeamDetails } from './components/TeamDetails';
import styles from './Scoreboard.module.css';

export const CurrentGameScoreboard = () => {
    const { currentGame } = useCurrentGame();
  
    return (
      <GameStateContextProvider gameId={currentGame?.id}>
        <Scoreboard />
      </GameStateContextProvider>
    );
};

export const Scoreboard = () => {

    const gameStage = useGameState<GameStageState>("GameStageState") ?? { stage: Stage.BeforeGame, periodNumber: 0, jamNumber: 0 };

    return (
        <>
            <div className={styles.scoreboardPage}>
                <div className={styles.sidePadding}></div>
                <div className={styles.centerContent}>
                    <div className={styles.clocksAndScores}>
                        <div className={styles.teamDetailsContainer}>
                            <TeamDetails side={TeamSide.Home} />
                            <TeamDetails side={TeamSide.Away} />
                        </div>
                        <div className={styles.clockContainer}>
                            <ScoreboardComponent className={styles.clock} header={`Period ${gameStage.periodNumber}`}>
                                <PeriodClock textClassName={styles.clockText} />
                            </ScoreboardComponent>
                            <ScoreboardComponent className={styles.clock} header={`Jam ${gameStage.jamNumber}`}>
                                <JamClock textClassName={styles.clockText} />
                            </ScoreboardComponent>
                        </div>
                    </div>
                </div>
                <div className={styles.sidePadding}></div>
            </div>
        </>
    );
}