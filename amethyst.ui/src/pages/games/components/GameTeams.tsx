import { Card, CardContent } from "@/components/ui";
import { useEvents, useTeamDetailsState } from "@/hooks";
import { GameTeam, TeamSide } from "@/types";
import { TeamDetails } from "./TeamDetails";
import { TeamSet } from "@/types/events";

type GameTeamsProps = {
    gameId: string;
}

export const GameTeams = ({ gameId }: GameTeamsProps) => {

    const { sendEvent } = useEvents();

    const homeTeam = useTeamDetailsState(TeamSide.Home);
    const awayTeam = useTeamDetailsState(TeamSide.Away);

    if(!homeTeam || ! awayTeam) {
        return (<></>);
    }

    const handleTeamChanged = (side: TeamSide, team: GameTeam) => {
        sendEvent(gameId, new TeamSet(side, team));
    }

    return (
        <Card>
            <CardContent className="flex gap-2 w-full flex-col xl:flex-row mt-6">
                <TeamDetails team={homeTeam.team} className="w-full xl:w-1/2" onTeamChanged={team => handleTeamChanged(TeamSide.Home, team)} />
                <TeamDetails team={awayTeam.team} className="w-full xl:w-1/2" onTeamChanged={team => handleTeamChanged(TeamSide.Away, team)} />
            </CardContent>
        </Card>
    )
}