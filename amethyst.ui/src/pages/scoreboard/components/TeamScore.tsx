import { useJamStatsState, useTeamScoreState } from "@/hooks";
import { ScaledText } from "../../../components/ScaledText";
import { TeamSide } from "@/types";
import { ScoreboardComponent } from "./ScoreboardComponent";
import { cn } from "@/lib/utils";
import { Star, StarOff } from "lucide-react";

type TeamScoreProps = {
    side: TeamSide,
    textClassName?: string,
};

export const TeamScore = ({ side, textClassName }: TeamScoreProps) => {

    const score = useTeamScoreState(side);
    const { lead, lost } = useJamStatsState(side) ?? { lead: false };
    

    return (
        <ScoreboardComponent className={cn("grow-[8] relative p-1")}>
            { lead &&
                <div className="absolute left-0 right-0 top-1 md:top-2 lg:top-3 xl:top-[2vh] flex justify-center">
                    { lost ? <StarOff className="w-full h-[5vh]" /> : <Star className="w-full h-[5vh]" /> }
                </div>
            }
            <ScaledText text={(score?.score ?? 0).toString()} className={cn("font-bold flex justify-center items-center h-full overflow-hidden", textClassName)} />
        </ScoreboardComponent>
    );
}