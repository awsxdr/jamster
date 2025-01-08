import { useConfiguration, useTeamDetailsState } from "@/hooks";
import { DisplayConfiguration, TeamSide } from "@/types";
import { useMemo } from "react";

export const TeamColorGradients = () => {

    const homeTeam = useTeamDetailsState(TeamSide.Home);
    const awayTeam = useTeamDetailsState(TeamSide.Away);

    const { showSidebars } = useConfiguration<DisplayConfiguration>("DisplayConfiguration") ?? { showSidebars: true };

    const homeTeamColor = useMemo(
        () => showSidebars ? (homeTeam?.team.color.shirtColor ?? '#000000') : "rgba(0,0,0,0)",
        [homeTeam, showSidebars]);

    const awayTeamColor = useMemo(
        () => showSidebars ? (awayTeam?.team.color.shirtColor ?? '#000000') : "rgba(0,0,0,0)",
        [awayTeam, showSidebars]);
    
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