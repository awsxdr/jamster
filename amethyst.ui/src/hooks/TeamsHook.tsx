import { StringMap, Team } from "@/types";
import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useTeamApi } from "./TeamApiHook";
import { useHubConnection } from "./SignalRHubConnection";

type TeamListChanged = () => void;
type TeamListWatch = (onTeamListChanged: TeamListChanged) => void;

type TeamChanged = () => void;
type TeamWatch = (teamId: string, onTeamChanged: TeamChanged) => void;

type TeamListContextProps = {
    teamsListNotifiers: TeamListChanged[];
    watchTeamsList: TeamListWatch;

    teamNofitiers: StringMap<TeamChanged[]>;
    watchTeam: TeamWatch;

    teams: StringMap<Team>;
}

const TeamListContext = createContext<TeamListContextProps>({
    teamsListNotifiers: [],
    watchTeamsList: () => { throw new Error('watchTeamsList called before context created'); },
    teamNofitiers: {},
    watchTeam: () => { throw new Error('watchTeam called before context created'); },
    teams: {},
});

export const useTeamList = () => {
    const context = useContext(TeamListContext);

    const teams = useMemo(() => Object.values(context.teams).filter(t => !!t), [context.teams]);

    return teams;
}

export const useTeam = (teamId: string) => {
    const context = useContext(TeamListContext);
    const [team, setTeam] = useState<Team>();
    const { getTeam } = useTeamApi();

    const getInitialState = useCallback(async () => {
        return await getTeam(teamId);
    }, [teamId]);

    useEffect(() => {
        getInitialState().then(setTeam);
    }, []);

    useEffect(() => {
        context.watchTeam(teamId, () => setTeam(context.teams[teamId]));
    }, []);

    return team;
}

export const TeamListContextProvider = ({ children }: PropsWithChildren) => {
    const [teamsListNotifiers, setTeamsListNotifiers] = useState<TeamListChanged[]>([]);
    const [teamNofitiers, setTeamNotifiers] = useState<StringMap<TeamChanged[]>>({});
    const [teams, setTeams] = useState<StringMap<Team>>({});

    const connection = useHubConnection('teams');

    const teamApi = useTeamApi();

    const getInitialState = useCallback(async () => {
        return await teamApi.getTeams();
    }, []);

    useEffect(() => {
        getInitialState().then(teams => {
            setTeams(
                teams.reduce((teams, team) => ({ ...teams, [team.id]: team }), {})
            );
            teamsListNotifiers.forEach(n => n());
        });
    }, []);

    const watchTeamsList = (onTeamListChanged: TeamListChanged) => {
        setTeamsListNotifiers(notifiers => [
            ...notifiers,
            onTeamListChanged,
        ]);
    }

    const watchTeam = (teamId: string, onTeamChanged: TeamChanged) => {
        setTeamNotifiers(notifiers => ({
            ...notifiers,
            [teamId]: [...(notifiers[teamId] ?? []), onTeamChanged]
        }));
    }

    useEffect(() => {
        connection?.invoke("WatchTeamChanged");
        connection?.invoke("WatchTeamCreated");
        connection?.invoke("WatchTeamArchived");
    }, [connection]);

    useEffect(() => {
        (async () => {
            connection?.invoke("WatchTeamChanged");
            connection?.invoke("WatchTeamCreated");
            connection?.invoke("WatchTeamArchived");
        })();
    }, [connection]);

    useEffect(() => {
        connection?.on("TeamCreated", (team: Team) => {
            setTeams(t => ({ ...t, [team.id]: team }));
            teamsListNotifiers.forEach(n => n());
        });

        connection?.on("TeamArchived", (teamId: string) => {
            setTeams(t => Object.keys(t).filter(k => k !== teamId).reduce((l, k) => ({ ...l, [k]: t[k] }), {}));
            teamsListNotifiers.forEach(n => n());
        });

        connection?.on("TeamChanged", (team: Team) => {
            setTeams(t => ({ ...t, [team.id]: team }));
            teamsListNotifiers.forEach(n => n());
            teamNofitiers[team.id]?.forEach(n => n());
        });
    });

    return (
        <TeamListContext.Provider value={{ watchTeamsList, teamsListNotifiers, watchTeam, teamNofitiers, teams }}>
            { children }
        </TeamListContext.Provider>
    );
}