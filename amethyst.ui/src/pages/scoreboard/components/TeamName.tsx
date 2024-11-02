import { useGameState } from "@/hooks";
import { ScaledText } from "../../../components/ScaledText";
import { useMemo } from "react";
import { TeamDetailsState, TeamSide } from "@/types";
import styles from './TeamName.module.css';
import { cn } from "@/lib/utils";

type TeamScoreProps = {
    side: TeamSide,
    textClassName?: string,
};

export const TeamName = ({ side, textClassName }: TeamScoreProps) => {

    const team = useGameState<TeamDetailsState>(`TeamDetailsState_${TeamSide[side]}`);

    const teamName = useMemo(() => {
        if(!team) {
            return '';
        }

        return team.team.names['scoreboard'] || team.team.names['default'] || '';
    }, [team]);

    return (
        <>
            <ScaledText text={teamName || ''} className={cn(styles.teamNameText, textClassName)} />
        </>
    );
}