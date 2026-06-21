import { StringMap, Team } from "@/types";
import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useRef, useState } from "react";
import { teamApi } from "./TeamApi";
import { useHubConnection } from "./SignalRHubConnection";
import { v4 as uuidv4 } from 'uuid';

type TeamChanged = (team?: Team) => void;

type TeamListContextProps = {
    watchTeam: (teamId: string, onTeamChanged: TeamChanged) => string;
    unwatchTeam: (teamId: string, watchId: string) => void;

    teams: StringMap<Team>;
}

type TeamListNotifierMap = StringMap<StringMap<TeamChanged>>;

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

    const getInitialState = useCallback(async () => {
        return await teamApi.getTeam(teamId);
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
    const [teams, setTeams] = useState<StringMap<Team>>({});

    const teamNotifiersRef = useRef<TeamListNotifierMap>({});

    const { connection } = useHubConnection('teams');

    const getInitialState = useCallback(async () => {
        return await teamApi.getTeams();
    }, []);

    useEffect(() => {
        getInitialState().then(teams => {
            setTeams(
                teams.reduce((teams, team) => ({ ...teams, [team.id]: team }), {})
            );

            Object.keys(teamNotifiersRef.current).forEach(teamId =>
                Object.values(teamNotifiersRef.current[teamId] ?? {}).forEach(notifier =>
                    notifier?.(teams.filter(t => t.id === teamId)[0])
                )
            )
        });
    }, []);

    const watchTeam = useCallback((teamId: string, onTeamChanged: TeamChanged) => {
        const handle = uuidv4();

        teamNotifiersRef.current[teamId] = {
            ...(teamNotifiersRef.current[teamId] ?? {}),
            [handle]: onTeamChanged
        };

        return handle;
    }, []);

    const unwatchTeam = useCallback((teamId: string, handle: string) => {
        if(!teamNotifiersRef.current[teamId]?.[handle]) {
            console.warn("Attempt to unwatch team list change with invalid handle", handle);
            return;
        }

        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        const { [handle]: _, ...newNotifiers } = teamNotifiersRef.current[teamId];

        teamNotifiersRef.current[teamId] = newNotifiers;
    }, []);

    useEffect(() => {
        (async () => {
            await connection?.invoke("WatchTeamChanged");
            await connection?.invoke("WatchTeamCreated");
            await connection?.invoke("WatchTeamArchived");
        })();

        connection?.onreconnected(() => {
            connection?.invoke("WatchTeamChanged");
            connection?.invoke("WatchTeamCreated");
            connection?.invoke("WatchTeamArchived");
        });
    }, [connection]);

    const notify = useCallback((teamId: string, team?: Team) => {
        Object.values(teamNotifiersRef.current[teamId] ?? {}).forEach(n => n?.(team));
    }, []);

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

    const context = useMemo(
        () => ({ watchTeam, unwatchTeam, teams }),
        [watchTeam, unwatchTeam, teams]
    );

    return (
        <TeamListContext.Provider value={context}>
            { children }
        </TeamListContext.Provider>
    );
}