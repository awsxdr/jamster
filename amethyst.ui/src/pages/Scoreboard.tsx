import { JamClock, LineupClock, PeriodClock } from '@components/clocks';
import { ScoreboardComponent } from '@components/ScoreboardComponent';
import styles from './Scoreboard.module.css';
import { useGameState } from '@hooks/StateHook';
import { TeamScore, TeamSide } from '@components/TeamScore';
import { PassScore } from '@/components/PassScore';
import { TeamName } from '@/components/TeamName';
import { TeamTimeouts } from '@/components/TeamTimeouts';

type GameStageState = {
    stage: Stage,
    periodNumber: number,
    jamNumber: number,
};

enum Stage {
    BeforeGame,
    Lineup,
    Jam,
    Timeout,
    Intermission,
    AfterGame,
}

export const Scoreboard = () => {

    const gameStage = useGameState<GameStageState>("GameStageState") ?? { stage: Stage.BeforeGame, periodNumber: 0, jamNumber: 0 };

    return (
        <>
            <div className={styles.scoreboardPage}>
                <div className={styles.sidePadding}></div>
                <div className={styles.centerContent}>
                    <div className={styles.teamNamesContainer}>
                        <TeamName side={TeamSide.Home} textClassName={styles.teamNameText} />
                        <TeamName side={TeamSide.Away} textClassName={styles.teamNameText} />
                    </div>
                    <div className={styles.clocksAndScores}>
                        <div className={styles.scoresContainer}>
                            <div className={styles.teamScoreContainer}>
                                <TeamTimeouts side={TeamSide.Home} />
                                <ScoreboardComponent className={styles.teamScore}>
                                    <TeamScore side={TeamSide.Home} textClassName={styles.teamScoreText} />
                                </ScoreboardComponent>
                                <ScoreboardComponent className={styles.passScore}>
                                    <PassScore side={TeamSide.Home} textClassName={styles.passScoreText} />
                                </ScoreboardComponent>
                            </div>
                            
                            <div className={styles.teamScoreContainer}>
                                <ScoreboardComponent className={styles.passScore}>
                                    <PassScore side={TeamSide.Away} textClassName={styles.passScoreText} />
                                </ScoreboardComponent>
                                <ScoreboardComponent className={styles.teamScore}>
                                    <TeamScore side={TeamSide.Away} textClassName={styles.teamScoreText} />
                                </ScoreboardComponent>
                                <TeamTimeouts side={TeamSide.Away} />
                            </div>
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