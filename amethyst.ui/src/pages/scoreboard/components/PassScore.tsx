import { useGameState } from "@/hooks";
import { ScaledText } from "@components/ScaledText";
import { ScoreboardComponent } from "./ScoreboardComponent";
import { TeamSide } from "@/types";

import styles from './PassScore.module.scss';
import { cn } from "@/lib/utils";

type PassScoreProps = {
    side: TeamSide,
    textClassName?: string,
};

type PassScoreState = {
    score: number,
};

export const PassScore = ({ side, textClassName }: PassScoreProps) => {

    const score = useGameState<PassScoreState>(`PassScoreState_${TeamSide[side]}`);

    return (
        <ScoreboardComponent className={styles.passScore}>
            <ScaledText text={(score?.score ?? 0).toString()} className={cn(styles.passScoreText, textClassName)} />
        </ScoreboardComponent>
    );
}