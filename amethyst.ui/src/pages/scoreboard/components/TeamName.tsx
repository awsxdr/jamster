import { useTeamDetailsState } from "@/hooks";
import { ScaledText } from "../../../components/ScaledText";
import { useMemo } from "react";
import { TeamSide } from "@/types";
import { cn } from "@/lib/utils";

type TeamScoreProps = {
    side: TeamSide,
    textClassName?: string,
};

export const TeamName = ({ side, textClassName }: TeamScoreProps) => {

    const team = useTeamDetailsState(side);

    const teamName = useMemo(() => {
        if(!team) {
            return '';
        }

        return team.team.names['scoreboard'] || team.team.names['default'] || '';
    }, [team]);

    return (
        <>
            <ScaledText text={teamName || ''} className={cn("flex justify-center items-center h-full m-2 overflow-hidden text-white", textClassName)} />
        </>
    );
}