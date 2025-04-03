import { createContext, PropsWithChildren, useContext, useEffect, useState } from "react";
import { useHubConnection } from "./SignalRHubConnection";
import { ClientActivity } from "@/types";
import { createBrowserRouter, RouteObject, useNavigate } from "react-router-dom";
import { v4 as uuidv4 } from 'uuid';

type CallbackHandle = string;

type ClientConnectionContextProps = {
    setActivity: (activity: ClientActivity, path: string, gameId: string | null) => void;
    watchActivityChange: (handler: ChangeActivityHandler) => CallbackHandle;
    unwatchActivityChange: (handle: CallbackHandle) => void;
}

const ClientConnectionContext = createContext<ClientConnectionContextProps>({
    setActivity: () => { throw new Error('setActivity called before context created'); },
    watchActivityChange: () => { throw new Error('watchActivityChange called before context created'); },
    unwatchActivityChange: () => { throw new Error('unwatchActivityChange called before context created'); },
});

const useActivity = (activity: ClientActivity, path: string, gameId: string | null) => {
    const context = useContext(ClientConnectionContext);

    if (context === undefined) {
        throw new Error('useChangeActivity must be used inside a ClientConnectionContextProvider');
    }

    context.setActivity(activity, path, gameId);
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

type ChangeActivityHandler = (activity: ClientActivity, gameId: string | null) => void;
type ChangeActivityNotifier = { [handle: CallbackHandle]: ChangeActivityHandler };

export const ClientConnectionContextProvider = ({ children }: PropsWithChildren) => {
    const { connection } = useHubConnection("clients");
    const [changeActivityNotifiers, setChangeActivityNotifiers] = useState<ChangeActivityNotifier>({});

    const handleChangeActivity = (activity: ClientActivity, gameId: string | null) => {
        Object.values(changeActivityNotifiers).forEach(notifier => {
            notifier(activity, gameId);
        })
    }

    const setActivity = (activity: ClientActivity, path: string, gameId: string | null) => {
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

            const { [handle]: _, ...newNotifier} = n;

            return newNotifier;
        });
    }

    connection?.on("ChangeActivity", handleChangeActivity);

    return (
        <ClientConnectionContext.Provider value={{ setActivity, watchActivityChange, unwatchActivityChange }}>
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

    useActivity(activity, path, null);

    useChangeActivity((newActivity, gameId) => {
        const path = `${getActivityPath(newActivity)}${gameId !== null && `?gameId=${gameId}`}`;

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