import { Skater, Team, UpdateTeam } from "@/types"
import { CreateTeam } from "@/types/CreateTeam";
import { Roster } from "@/types/Roster";
import { API_URL } from "@/constants";

type TeamApi = {
    getTeams: () => Promise<Team[]>;
    getTeam: (teamId: string) => Promise<Team>;
    createTeam: (team: CreateTeam) => Promise<Team>;
    setTeam: (teamId: string, team: UpdateTeam) => Promise<void>;
    deleteTeam: (teamId: string) => Promise<void>;
    getRoster: (teamId: string) => Promise<Roster>;
    setRoster: (teamId: string, roster: Skater[]) => Promise<void>;
}

export const useTeamApi: () => TeamApi = () => {

    const getTeams = async () => {
        const response = await fetch(`${API_URL}/api/teams`);
        return (await response.json()) as Team[];
    }

    const getTeam = async (teamId: string) => {
        const response = await fetch(`${API_URL}/api/teams/${teamId}`);
        return (await response.json()) as Team;
    }

    const createTeam = async (team: CreateTeam) => {
        const response = await fetch(
            `${API_URL}/api/teams`,
            {
                method: 'POST',
                body: JSON.stringify(team),
                headers: {
                    "Content-Type": "application/json; charset=utf-8"
                }
            }
        );
        return (await response.json()) as Team;
    }

    const setTeam = async (teamId: string, team: UpdateTeam) => {
        await fetch(
            `${API_URL}/api/teams/${teamId}`,
            {
                method: 'PUT',
                body: JSON.stringify(team),
                headers: {
                    "Content-Type": "application/json; charset=utf-8"
                }
            }
        );
    }

    const deleteTeam = async (teamId: string) => {
        await fetch(`${API_URL}/api/teams/${teamId}`, { method: 'DELETE' });
    }

    const getRoster = async (teamId: string) => {
        const response = await fetch(`${API_URL}/api/teams/${teamId}/roster`);
        return (await response.json()) as Roster;
    }

    const setRoster = async (teamId: string, roster: Skater[]) => {
        await fetch(
            `${API_URL}/api/teams/${teamId}/roster`,
            {
                method: 'PUT',
                body: JSON.stringify({ roster }),
                headers: {
                    "Content-Type": "application/json; charset=utf-8"
                }
            }
        );
    }

    return {
        getTeams,
        getTeam,
        createTeam,
        setTeam,
        deleteTeam,
        getRoster,
        setRoster,
    };
}