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
        <div 
            className="flex justify-center h-full p-5 overflow-hidden rounded-3xl"
            style={{ color: foreground, backgroundColor: background }}
        >
            <ScaledText 
                text={teamName || ''} 
                className={cn(
                    "flex items-center text-center font-bold h-full",
                    textClassName
                )} 
            />
        </div>
    );
}