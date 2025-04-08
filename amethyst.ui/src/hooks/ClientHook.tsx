import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useState } from "react";
import { useHubConnection } from "./SignalRHubConnection";
import { Client, ClientActivity } from "@/types";
import { createBrowserRouter, RouteObject, useLocation, useNavigate } from "react-router-dom";
import { v4 as uuidv4 } from 'uuid';
import { useClientsApi } from "./ClientsApi";

type CallbackHandle = string;

type ClientConnectionContextProps = {
    clients: Client[];
    hasConnection: boolean;
    activity: ClientActivity;
    path: string;
    gameId: string | null;
    setActivity: (activity: ClientActivity, path: string, gameId: string | null) => void;
    watchActivityChange: (handler: ChangeActivityHandler) => CallbackHandle;
    unwatchActivityChange: (handle: CallbackHandle) => void;
}

const ClientConnectionContext = createContext<ClientConnectionContextProps>({
    clients: [],
    hasConnection: false,
    activity: ClientActivity.Unknown,
    path: "",
    gameId: null,
    setActivity: () => { throw new Error('setActivity called before context created'); },
    watchActivityChange: () => { throw new Error('watchActivityChange called before context created'); },
    unwatchActivityChange: () => { throw new Error('unwatchActivityChange called before context created'); },
});

const useActivity = (activity: ClientActivity, path: string, gameId: string | null) => {
    const context = useContext(ClientConnectionContext);

    if (context === undefined) {
        throw new Error('useChangeActivity must be used inside a ClientConnectionContextProvider');
    }

    useEffect(() => {
        context.setActivity(activity, path, gameId);
    }, [context.hasConnection, activity, path, gameId]);
}

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

export const useClients = () => {
    const { clients } = useContext(ClientConnectionContext);

    return clients;
}

type ChangeActivityHandler = (activity: ClientActivity, gameId: string | null) => void;
type ChangeActivityNotifier = { [handle: CallbackHandle]: ChangeActivityHandler };

export const ClientConnectionContextProvider = ({ children }: PropsWithChildren) => {
    const { connection } = useHubConnection("clients");
    const [changeActivityNotifiers, setChangeActivityNotifiers] = useState<ChangeActivityNotifier>({});
    const [clients, setClients] = useState<Client[]>([]);
    const [clientActivity, setClientActivity] = useState(ClientActivity.Unknown);
    const [path, setPath] = useState("");
    const [gameId, setGameId] = useState<string | null>(null);

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
        (async () => {
            connection?.onreconnected(() => {
                connection.send("SetActivity", clientActivity, path, gameId);
            });
        })();
    }, [connection, changeActivityNotifiers]);


    const handleChangeActivity = (activity: ClientActivity, gameId: string | null) => {
        Object.values(changeActivityNotifiers).forEach(notifier => {
            notifier(activity, gameId);
        })
    }

    const setActivity = (activity: ClientActivity, path: string, gameId: string | null) => {

        setClientActivity(activity);
        setPath(path);
        setGameId(gameId);

        connection?.send("SetActivity", activity, path, gameId);
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

    connection?.on("ChangeActivity", handleChangeActivity);

    return (
        <ClientConnectionContext.Provider 
            value={{ 
                clients, 
                hasConnection: !!connection, 
                activity: clientActivity,
                path,
                gameId,
                setActivity, 
                watchActivityChange, 
                unwatchActivityChange 
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

const ClientActivityRoute = ({ activity, path, getActivityPath, children }: PropsWithChildren<ClientActivityRouteProps>) => {

    const navigate = useNavigate();
    const location = useLocation();

    const gameId = location.search.match(/[?&]gameId=([^&]+)/)?.[1] ?? null;

    useActivity(activity, path, gameId); 

    useChangeActivity((newActivity, gameId) => {
        const path = `${getActivityPath(newActivity)}${gameId !== null ? `?gameId=${gameId}` : ""}`;

        navigate(path);
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