import { useTeamScoreState } from "@/hooks";
import { ScaledText } from "../../../components/ScaledText";
import { TeamSide } from "@/types";

type TeamScoreProps = {
    side: TeamSide,
};

export const TeamScore = ({ side }: TeamScoreProps) => {

    const score = useTeamScoreState(side);

    return (
        <ScaledText 
            text={(score?.score ?? 0).toString()} 
            className="flex justify-center items-end w-1/2 md:w-1/4 h-[10vh] m-1" />
    );
}