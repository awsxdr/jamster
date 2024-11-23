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
        <ScoreboardComponent className="grow w-[10%] h-[40%] m-1">
            <ScaledText 
                text={jamScore.toString()} 
                className={cn("flex justify-center items-center h-full m-0.5 overflow-hidden", textClassName)} 
            />
        </ScoreboardComponent>
    );
}