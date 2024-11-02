import { useGameState } from "@/hooks";
import { ScaledText } from "../../../components/ScaledText";
import { TeamSide } from "@/types";
import { ScoreboardComponent } from "./ScoreboardComponent";
import { cn } from "@/lib/utils";

import styles from './TeamScore.module.scss';

type TeamScoreProps = {
    side: TeamSide,
    textClassName?: string,
};

type TeamScoreState = {
    score: number,
};

export const TeamScore = ({ side, textClassName }: TeamScoreProps) => {

    const score = useGameState<TeamScoreState>(`TeamScoreState_${TeamSide[side]}`);

    return (
        <ScoreboardComponent className={styles.teamScore}>
            <ScaledText text={(score?.score ?? 0).toString()} className={cn(styles.teamScoreText, textClassName)} />
        </ScoreboardComponent>
    );
}