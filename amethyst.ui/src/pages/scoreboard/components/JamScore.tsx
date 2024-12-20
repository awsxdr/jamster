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
        <ScoreboardComponent className="grow w-[20%] h-[40%] max-w-[15%] p-2">
            <ScaledText 
                text={jamScore.toString()} 
                className={cn("flex justify-center items-center h-full overflow-hidden", textClassName)} 
            />
        </ScoreboardComponent>
    );
}