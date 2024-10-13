import { useGameState } from "@/hooks";
import { ScaledText } from "./ScaledText";
import { TeamSide } from "./TeamScore";

type PassScoreProps = {
    side: TeamSide,
    textClassName?: string,
};

type PassScoreState = {
    score: number,
};

export const PassScore = ({ side, textClassName }: PassScoreProps) => {

    const gameState = useGameState();
    const score = gameState.useStateWatch<PassScoreState>(`PassScoreState_${TeamSide[side]}`);

    return (
        <>
            <ScaledText text={(score?.score ?? 0).toString()} className={textClassName} />
        </>
    );
}