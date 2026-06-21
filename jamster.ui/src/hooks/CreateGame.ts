import { Team, TeamSide } from "@/types";
import { eventsApi, gameApi, teamApi } from ".";
import { TeamSet } from "@/types/events";

export const useCreateGame = () => {
    return async (
        homeTeamId: string,
        homeTeamColorIndex: number,
        awayTeamId: string,
        awayTeamColorIndex: number,
        gameName: string
    ) => {
        const gameId = await gameApi.createGame(gameName);

        const defaultColor = {
            name: 'Black',
            shirtColor: '#000000',
            complementaryColor: '#ffffff',
        };

        const homeTeam = await teamApi.getTeam(homeTeamId);
        const awayTeam = await teamApi.getTeam(awayTeamId);

        const getTeamColor = (team: Team, colorIndex: number) => {
            const colorKeys = Object.keys(team.colors);
            if(colorKeys.length === 0) {
                return defaultColor;
            }

            if(colorIndex > colorKeys.length) {
                colorIndex = 0;
            }

            const color = 
                team.colors[colorKeys[colorIndex]]
                ?? defaultColor;

            return { name: colorKeys[colorIndex], ...color };
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
        
        await eventsApi.sendEvent(gameId, new TeamSet(TeamSide.Home, homeGameTeam));
        await eventsApi.sendEvent(gameId, new TeamSet(TeamSide.Away, awayGameTeam));

        return gameId;
    };
}