import { useTripScoreState } from "@/hooks";
import { ScaledText } from "@components/ScaledText";
import { ScoreboardComponent } from "./ScoreboardComponent";
import { TeamSide } from "@/types";

import styles from './PassScore.module.scss';
import { cn } from "@/lib/utils";

type PassScoreProps = {
    side: TeamSide,
    textClassName?: string,
};

export const PassScore = ({ side, textClassName }: PassScoreProps) => {

    const score = useTripScoreState(side);

    return (
        <ScoreboardComponent className={styles.passScore}>
            <ScaledText text={(score?.score ?? 0).toString()} className={cn(styles.passScoreText, textClassName)} />
        </ScoreboardComponent>
    );
}