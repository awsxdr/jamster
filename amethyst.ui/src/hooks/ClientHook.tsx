import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useHubConnection } from "./SignalRHubConnection";
import { ActivityData, Client, ClientActivity } from "@/types";
import { createBrowserRouter, RouteObject, useNavigate, useSearchParams } from "react-router-dom";
import { v4 as uuidv4 } from 'uuid';
import { useClientsApi } from "./ClientsApi";

type CallbackHandle = string;

type ClientConnectionContextProps = {
    clients: Client[];
    hasConnection: boolean;
    activity: ActivityData;
    setActivity: (activity: ActivityData) => void;
    watchActivityChange: (handler: ChangeActivityHandler) => CallbackHandle;
    unwatchActivityChange: (handle: CallbackHandle) => void;
    getName: () => Promise<string>;
    setName: (clientName: string) => Promise<void>;
}

type ChangeActivityHandler = (activity: ActivityData) => void;
type ChangeActivityNotifier = { [handle: CallbackHandle]: ChangeActivityHandler };

const ClientConnectionContext = createContext<ClientConnectionContextProps>({
    clients: [],
    hasConnection: false,
    activity: { activity: ClientActivity.Unknown, gameId: null, languageCode: "en" },
    setActivity: () => { throw new Error('setActivity called before context created'); },
    watchActivityChange: () => { throw new Error('watchActivityChange called before context created'); },
    unwatchActivityChange: () => { throw new Error('unwatchActivityChange called before context created'); },
    getName: () => { throw new Error('getName called before context created'); },
    setName: () => { throw new Error('setName called before context created'); },
});

const useChangeActivity = (onActivityChangeRequested: ChangeActivityHandler) => {
    const context = useContext(ClientConnectionContext);

    if (context === undefined) {
        throw new Error('useChangeActivity must be used inside a ClientConnectionContextProvider');
    }

    useEffect(() => {
        const handle = context.watchActivityChange(onActivityChangeRequested);

        return () => context.unwatchActivityChange(handle);
    }, []);
}

export const useClientName = () => {
    const context = useContext(ClientConnectionContext);
    const [name, setName] = useState("");

    const clients = useClients();

    useEffect(() => {
        context.getName().then(setName);
    }, [clients]);

    const setClientName = (name: string) => context.setName(name);

    return { name, setName: setClientName };
}

export const useClients = () => {
    const { clients } = useContext(ClientConnectionContext);

    return clients;
}

export const ClientConnectionContextProvider = ({ children }: PropsWithChildren) => {
    const { connection } = useHubConnection("clients");
    const [changeActivityNotifiers, setChangeActivityNotifiers] = useState<ChangeActivityNotifier>({});
    const [clients, setClients] = useState<Client[]>([]);
    const [clientActivity, setClientActivity] = useState<ActivityData>({ activity: ClientActivity.Unknown, gameId: null, languageCode: "en" });
    const [clientName, setClientName] = useState("");

    const clientsApi = useClientsApi();

    const getInitialState = useCallback(async () => {
        return await clientsApi.getConnectedClients();
    }, []);

    useEffect(() => {
        getInitialState().then(setClients);
    }, []);

    useEffect(() => {
        if(!connection) {
            return;
        }

        connection.invoke("WatchClientsList");

        connection.on("ConnectedClientsChanged", clients => {
            setClients(clients);
        });

        return () => connection.off("ConnectedClientsChanged");
    }, [connection]);

    useEffect(() => {
        connection?.onreconnected(() => {
            connection.send("SetActivity", clientActivity);
        });
    }, [connection, changeActivityNotifiers]);

    useEffect(() => {
        if(!connection) {
            return;
        }

        setActivity(clientActivity);

        const sessionName = sessionStorage.getItem("clientName");

        if(!sessionName) {
            (async () => {
                const name = await getName();

                setClientName(name);
                sessionStorage.setItem("clientName", name);
            })();

            return;
        }

        setName(sessionName);
        
    }, [connection]);

    const handleChangeActivity = (activity: ActivityData) => {
        Object.values(changeActivityNotifiers).forEach(notifier => {
            notifier(activity);
        });
    }

    const setActivity = (activity: ActivityData) => {
        setClientActivity(activity);

        connection?.send("SetActivity", activity);
    }

    const watchActivityChange = (handler: ChangeActivityHandler) => {
        const newId = uuidv4();

        setChangeActivityNotifiers(n => ({
            ...n,
            [newId]: handler,
        }));

        return newId;
    }

    const unwatchActivityChange = (handle: CallbackHandle) => {
        setChangeActivityNotifiers(n => {
            if(!n[handle]) {
                console.warn("Attempt to unwatch activity change with invalid handle", handle);
                return n;
            }

            // eslint-disable-next-line @typescript-eslint/no-unused-vars
            const { [handle]: _, ...newNotifier} = n;

            return newNotifier;
        });
    }

    const getName = async () => {
        const name = await connection?.invoke<string>("GetConnectionName");
        if(name && clientName !== name) {
            setClientName(name);
        }

        return name ?? "";
    }

    const setName = async (name: string) => {
        await connection?.invoke("SetConnectionName", name);
        setClientName(name);
    }

    connection?.on("ChangeActivity", handleChangeActivity);

    return (
        <ClientConnectionContext.Provider 
            value={{ 
                clients, 
                hasConnection: !!connection, 
                activity: clientActivity,
                setActivity, 
                watchActivityChange, 
                unwatchActivityChange,
                getName,
                setName,
            }}
        >
            { children }
        </ClientConnectionContext.Provider>
    )
}

type ClientActivityRouteObject = RouteObject & {
    activity: ClientActivity;
}

type ClientActivityRouteProps = {
    activity: ClientActivity;
    path: string;
    getActivityPath: (activity: ClientActivity) => string;
}

const getActivityData = (activity: ClientActivity, searchParams: URLSearchParams): ActivityData => {
    const gameId = searchParams.get("gameId");
    const languageCode = searchParams.get("languageCode") ?? "en";

    const otherProperties =
        activity === ClientActivity.Scoreboard ? {
            useSidebars: searchParams.get("useSidebars") === "true",
            useNameBackgrounds: searchParams.get("useNameBackgrounds") === "true",
        } :
        activity === ClientActivity.StreamOverlay ? { 
            scale: parseFloat(searchParams.get("scale") ?? "1.0"), 
            useBackground: searchParams.get("useBackground") === "true",
            backgroundColor: searchParams.get("backgroundColor") ?? "#00ff00",
        } :
        { };
    
    return { ...otherProperties, activity, gameId, languageCode } as ActivityData;
}

const ClientActivityRoute = ({ activity, getActivityPath, children }: PropsWithChildren<ClientActivityRouteProps>) => {

    const navigate = useNavigate();
    const [searchParams] = useSearchParams();

    const activityData = useMemo(() => getActivityData(activity, searchParams), [activity, searchParams]);

    const context = useContext(ClientConnectionContext);

    useEffect(() => {
        context.setActivity(activityData);
    }, [activityData]);

    useChangeActivity(newActivity => {
        const extraParams = 
            Object.keys(newActivity)
                .filter(k => !["gameId", "activity"].includes(k))
                .filter(k => newActivity[k as keyof typeof newActivity] !== undefined)
                .map(k => `${k}=${encodeURIComponent(newActivity[k as keyof typeof newActivity]!)}`);

        const params = [
            newActivity.gameId && `gameId=${newActivity.gameId}`,
            ...extraParams
        ].join("&");
        context.setActivity(newActivity);

        const path = `${getActivityPath(newActivity.activity)}?${params}`;
        navigate(path, { replace: true });
    });

    return children;
}

export const createClientActivityRouter = (routes: ClientActivityRouteObject[]) => {

    const getActivityPath = (activity: ClientActivity) =>
        routes.find(r => r.activity === activity)?.path
        ?? "/";

    return createBrowserRouter(routes.map(r => ({ ...r, element: r.element && (
        <ClientActivityRoute activity={r.activity} path={r.path ?? ""} getActivityPath={getActivityPath}>
            {r.element}
        </ClientActivityRoute>
    )})));
}