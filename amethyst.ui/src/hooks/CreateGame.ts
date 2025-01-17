import { Team, TeamSide } from "@/types";
import { useEvents, useGameApi, useTeamApi } from ".";
import { TeamSet } from "@/types/events";

export const useCreateGame = () => {
    const { createGame } = useGameApi();
    const { sendEvent } = useEvents();
    const { getTeam } = useTeamApi();

    return async (
        homeTeamId: string,
        homeTeamColorIndex: number,
        awayTeamId: string,
        awayTeamColorIndex: number,
        gameName: string
    ) => {
        const gameId = await createGame(gameName);

        const homeTeam = await getTeam(homeTeamId)
        const awayTeam = await getTeam(awayTeamId);

        const getTeamColor = (team: Team, colorIndex: number) => {
            const colorKeys = Object.keys(team.colors);
            if(colorKeys.length === 0) {
                return {
                    name: 'Black',
                    shirtColor: '#000000',
                    complementaryColor: '#ffffff',
                };
            }

            if(colorIndex > colorKeys.length) {
                colorIndex = 0;
            }

            return { name: colorKeys[colorIndex], ...team.colors[colorKeys[colorIndex]]! };
        }

        const homeTeamColor = getTeamColor(homeTeam, homeTeamColorIndex);
        const awayTeamColor = getTeamColor(awayTeam, awayTeamColorIndex);

        const homeGameTeam = {
            names: { ...homeTeam.names, "color": homeTeamColor.name },
            color: homeTeamColor,
            roster: homeTeam.roster.map(s => ({ ...s, isSkating: true })),
        };

        const awayGameTeam = {
            names: { ...awayTeam.names, "color": awayTeamColor.name },
            color: awayTeamColor,
            roster: awayTeam.roster.map(s => ({ ...s, isSkating: true })),
        };
        
        await sendEvent(gameId, new TeamSet(TeamSide.Home, homeGameTeam));
        await sendEvent(gameId, new TeamSet(TeamSide.Away, awayGameTeam));

        return gameId;
    };
}