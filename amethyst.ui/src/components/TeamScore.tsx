import { useGameState } from "@/hooks";
import { ScaledText } from "./ScaledText";

type TeamScoreProps = {
    side: TeamSide,
    textClassName?: string,
};

export enum TeamSide {
    Home,
    Away,
}

type TeamScoreState = {
    score: number,
};

export const TeamScore = ({ side, textClassName }: TeamScoreProps) => {

    const score = useGameState<TeamScoreState>(`TeamScoreState_${TeamSide[side]}`);

    return (
        <>
            <ScaledText text={(score?.score ?? 0).toString()} className={textClassName} />
        </>
    );
}