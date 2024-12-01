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

    const foreground = useMemo(
        () => team?.team.color.complementaryColor ?? '#ffffff',
        [team]);

    const background = useMemo(
        () => team?.team.color.shirtColor ?? '#000000',
        [team]);

    return (
        <>
            <ScaledText 
                text={teamName || ''} 
                className={cn(
                    "flex justify-center items-center text-center font-bold h-full m-2 overflow-hidden",
                    textClassName
                )} 
                style={{ color: foreground, backgroundColor: background }}
            />
        </>
    );
}