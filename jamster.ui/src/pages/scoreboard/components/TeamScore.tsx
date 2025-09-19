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
    const { lead: opponentLead } = useJamStatsState(side === TeamSide.Home ? TeamSide.Away : TeamSide.Home) ?? { lead: false };

    return (
        <ScoreboardComponent className={cn("grow-[8] relative p-1")}>
            <div className="absolute left-0 right-0 top-0 md:top-2 lg:top-3 2xl:top-2 flex justify-center">
                { lead && (
                    <>
                        { lost 
                            ? <StarOff className="w-full h-[7vh]" strokeWidth="3px" /> 
                            : <Star className="w-full h-[7vh]" strokeWidth="3px" /> 
                        }
                    </>
                )}
                { !lead && !opponentLead && !lost && (
                    <Star className="w-full h-[7vh] text-gray-200" strokeWidth="3px" />
                )}
            </div>
            <ScaledText 
                text={(score?.score ?? 0).toString()} 
                scale={1.4}
                className={cn(
                    "font-bold flex justify-center items-center h-full overflow-hidden",
                    "pt-4 md:pt-6 lg:pt-8 leading-none",
                    textClassName)} 
            />
        </ScoreboardComponent>
    );
}