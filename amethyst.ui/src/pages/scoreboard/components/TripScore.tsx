import { useTripScoreState } from "@/hooks";
import { ScaledText } from "@components/ScaledText";
import { ScoreboardComponent } from "./ScoreboardComponent";
import { TeamSide } from "@/types";

import { cn } from "@/lib/utils";

type TripScoreProps = {
    side: TeamSide,
    textClassName?: string,
};

export const TripScore = ({ side, textClassName }: TripScoreProps) => {

    const score = useTripScoreState(side);

    return (
        <ScoreboardComponent className="grow w-[10%] h-[40%] m-1">
            <ScaledText 
                text={(score?.score ?? 0).toString()} 
                className={cn("flex justify-center items-center h-full m-0.5 overflow-hidden", textClassName)} 
            />
        </ScoreboardComponent>
    );
}