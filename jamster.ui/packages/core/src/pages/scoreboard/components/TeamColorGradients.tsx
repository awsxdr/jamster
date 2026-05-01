import { useTeamDetailsState } from "@/hooks";
import { TeamSide } from "@/types";
import { useMemo } from "react";

type TeamColorGradientsProps = {
    hidden?: boolean;
}

export const TeamColorGradients = ({ hidden }: TeamColorGradientsProps) => {

    const homeTeam = useTeamDetailsState(TeamSide.Home);
    const awayTeam = useTeamDetailsState(TeamSide.Away);

    const homeTeamColor = useMemo(
        () => hidden ? "rgba(0,0,0,0)" : (homeTeam?.team.color.shirtColor ?? "rgba(0,0,0,0)"),
        [homeTeam, hidden]);

    const awayTeamColor = useMemo(
        () => hidden ? "rgba(0,0,0,0)" : (awayTeam?.team.color.shirtColor ?? "rgba(0,0,0,0)"),
        [awayTeam, hidden]);
    
    return (
        <div className="absolute left-0 top-0 w-full h-full bg-black">
            <div 
                className="absolute left-0 h-full w-2/5 max-w-[50vh]"
                style={{ background: `linear-gradient(to left, #000 0%, ${homeTeamColor} 100%)` }}
            >
            </div>
            <div 
                className="absolute right-0 h-full w-2/5 max-w-[50vh]"
                style={{ background: `linear-gradient(to right, #000 0%, ${awayTeamColor} 100%)` }}
            >
            </div>
        </div>
    )
}