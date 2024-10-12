import { JamClock } from '@/components/clocks/JamClock';
import { ScoreboardComponent } from '@/components/ScoreboardComponent';
import styles from './Scoreboard.module.css';
import { ScaledText } from '@/components/ScaledText';

export const Scoreboard = () => {

    return (
        <>
            <div className={styles.scoreboardPage}>
                <ScoreboardComponent className={styles.clock}>
                    <JamClock />
                </ScoreboardComponent>
                <ScoreboardComponent className={styles.clock}>
                    <ScaledText text='1234' />
                </ScoreboardComponent>
            </div>
        </>
    );
}