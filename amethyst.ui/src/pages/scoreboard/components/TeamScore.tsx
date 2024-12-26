import { useJamStatsState, useTeamScoreState } from "@/hooks";
import { ScaledText } from "../../../components/ScaledText";
import { TeamSide } from "@/types";
import { ScoreboardComponent } from "./ScoreboardComponent";
import { cn } from "@/lib/utils";
import { Star } from "lucide-react";

type TeamScoreProps = {
    side: TeamSide,
    textClassName?: string,
};

export const TeamScore = ({ side, textClassName }: TeamScoreProps) => {

    const score = useTeamScoreState(side);
    const { lead } = useJamStatsState(side) ?? { lead: false };
    

    return (
        <ScoreboardComponent className="grow-[8] p-2 relative">
            { lead &&
                <div className="absolute left-0 right-0 top-6 scale-150 flex justify-center">
                    <Star fill="#000" /> 
                </div>
            }
            <ScaledText text={(score?.score ?? 0).toString()} className={cn("font-bold flex justify-center items-center h-full overflow-hidden", textClassName)} />
        </ScoreboardComponent>
    );
}