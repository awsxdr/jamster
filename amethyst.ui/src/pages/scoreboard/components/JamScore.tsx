import { useTeamScoreState } from "@/hooks";
import { ScaledText } from "@components/ScaledText";
import { ScoreboardComponent } from "./ScoreboardComponent";
import { TeamSide } from "@/types";

import { cn } from "@/lib/utils";

type JamScoreProps = {
    side: TeamSide,
    textClassName?: string,
};

export const JamScore = ({ side, textClassName }: JamScoreProps) => {

    const { jamScore } = useTeamScoreState(side) ?? { jamScore: 0 };
    
    return (
        <ScoreboardComponent className="h-2/5 w-full p-2">
            <ScaledText 
                text={jamScore.toString()} 
                className={cn("flex justify-center items-center h-full overflow-hidden", textClassName)} 
            />
        </ScoreboardComponent>
    );
}