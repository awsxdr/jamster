import { useTeamScoreState } from "@/hooks";
import { TeamSide } from "@/types"

type JamScoreProps = {
    side: TeamSide;
}

export const JamScore = ({ side }: JamScoreProps) => {

    const score = useTeamScoreState(side);

    return (
        <div className="flex w-full justify-center items-center gap-2">
            Jam score: <span className="text-5xl">{score?.jamScore}</span>
        </div>
    )
}