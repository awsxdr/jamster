import { StringMap, Team } from "@/types";
import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useRef, useState } from "react";
import { useTeamApi } from "./TeamApiHook";
import { useHubConnection } from "./SignalRHubConnection";
import { v4 as uuidv4 } from 'uuid';

type TeamChanged = (team?: Team) => void;

type TeamListContextProps = {
    watchTeam: (teamId: string, onTeamChanged: TeamChanged) => string;
    unwatchTeam: (teamId: string, watchId: string) => void;

    teams: StringMap<Team>;
}

const TeamListContext = createContext<TeamListContextProps>({
    watchTeam: () => { throw new Error('watchTeam called before context created'); },
    unwatchTeam: () => { throw new Error('unwatchTeam called before context created'); },
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

    const setTeamRef = useRef(setTeam);

    useEffect(() => {
        getInitialState().then(setTeam);
        const watchId = context.watchTeam(teamId, team => {
            setTeamRef.current?.(team);
        });

        return () => context.unwatchTeam(teamId, watchId);
    }, []);

    useEffect(() => {
        setTeamRef.current = setTeam;
    }, [setTeam]);

    return team;
}

export const TeamListContextProvider = ({ children }: PropsWithChildren) => {
    const [teamNotifiers, setTeamNotifiers] = useState<StringMap<StringMap<TeamChanged>>>({});
    const [teams, setTeams] = useState<StringMap<Team>>({});

    const { connection } = useHubConnection('teams');

    const teamApi = useTeamApi();

    const getInitialState = useCallback(async () => {
        return await teamApi.getTeams();
    }, []);

    useEffect(() => {
        getInitialState().then(teams => {
            setTeams(
                teams.reduce((teams, team) => ({ ...teams, [team.id]: team }), {})
            );

            Object.keys(teamNotifiers).forEach(teamId =>
                Object.values(teamNotifiers[teamId] ?? {}).forEach(notifier =>
                    notifier?.(teams.filter(t => t.id === teamId)[0])
                )
            )
        });
    }, []);

    const watchTeam = (teamId: string, onTeamChanged: TeamChanged) => {
        const watchId = uuidv4();

        setTeamNotifiers(notifiers => ({
            ...notifiers,
            [teamId]: { ...notifiers[teamId], [watchId]: onTeamChanged }
        }));

        return watchId;
    }

    const unwatchTeam = (teamId: string, watchId: string) => {
        setTeamNotifiers(notifiers => ({
            ...notifiers,
            [teamId]: { ...notifiers[teamId], [watchId]: undefined }
        }));
    }

    useEffect(() => {
        (async () => {
            await connection?.invoke("WatchTeamChanged");
            await connection?.invoke("WatchTeamCreated");
            await connection?.invoke("WatchTeamArchived");
        })();
    }, [connection]);

    useEffect(() => {
        connection?.onreconnected(() => {
            connection?.invoke("WatchTeamChanged");
            connection?.invoke("WatchTeamCreated");
            connection?.invoke("WatchTeamArchived");
        });
    }, [connection]);

    const notify = useCallback((teamId: string, team?: Team) => {
        Object.values(teamNotifiers[teamId] ?? {}).forEach(n => n?.(team));
    }, [teamNotifiers, setTeamNotifiers]);

    useEffect(() => {
        connection?.on("TeamCreated", (team: Team) => {
            setTeams(t => ({ ...t, [team.id]: team }));
            notify(team.id, team);
        });

        connection?.on("TeamArchived", (teamId: string) => {
            setTeams(t => Object.keys(t).filter(k => k !== teamId).reduce((l, k) => ({ ...l, [k]: t[k] }), {}));
            notify(teamId);
        });

        connection?.on("TeamChanged", (team: Team) => {
            setTeams(t => ({ ...t, [team.id]: team }));
            notify(team.id, team);
        });

        return () => {
            connection?.off("TeamCreated");
            connection?.off("TeamArchived");
            connection?.off("TeamChanged");
        };
    }, [connection, notify]);

    return (
        <TeamListContext.Provider value={{ watchTeam, unwatchTeam, teams }}>
            { children }
        </TeamListContext.Provider>
    );
}