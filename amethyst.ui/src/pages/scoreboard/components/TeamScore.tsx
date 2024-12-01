import { useTeamScoreState } from "@/hooks";
import { ScaledText } from "../../../components/ScaledText";
import { TeamSide } from "@/types";
import { ScoreboardComponent } from "./ScoreboardComponent";
import { cn } from "@/lib/utils";

type TeamScoreProps = {
    side: TeamSide,
    textClassName?: string,
};

export const TeamScore = ({ side, textClassName }: TeamScoreProps) => {

    const score = useTeamScoreState(side);

    return (
        <ScoreboardComponent className="grow-[8]">
            <ScaledText text={(score?.score ?? 0).toString()} className={cn("font-bold flex justify-center items-center h-full m-2 overflow-hidden", textClassName)} />
        </ScoreboardComponent>
    );
}