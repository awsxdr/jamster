import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useState, useSyncExternalStore } from "react"
import * as SignalR from '@microsoft/signalr';

type SystemStateContextProps = {
    useCurrentGame: () => string | undefined,
};

const SystemStateContext = createContext<SystemStateContextProps>({
    useCurrentGame: () => { throw new Error("useCurrentGame used before context created"); },
});

export const useSystemState = () => useContext(SystemStateContext);

type SystemStateContextProviderProps = {};

type GameInfo = {
    id: string,
    name: string,
};

const API_URL = 'http://localhost:5249';

export const SystemStateContextProvider = ({ children }: PropsWithChildren<SystemStateContextProviderProps>) => {

    const [connection, setConnection] = useState<Promise<SignalR.HubConnection>>(Promise.reject("Connection not yet started"));
    
    useEffect(() => {
        const hubConnection = new SignalR.HubConnectionBuilder()
            .withUrl(`${API_URL}/api/hubs/system`, { withCredentials: false })
            .withAutomaticReconnect({ nextRetryDelayInMilliseconds: context => {
                if(context.previousRetryCount < 10) {
                    return 250;
                } else if(context.previousRetryCount < 40) {
                    return 1000;
                } else {
                    return 5000;
                }
            }})
            .build();

        setConnection(hubConnection.start().then(() => hubConnection));

        return () => {
            hubConnection.stop();
        }
    }, [setConnection]);

    useEffect(() => {
        (async () => {
            const resolvedConnection = await connection;
            resolvedConnection.onreconnected(() => {
                resolvedConnection.invoke("WatchSystemState");
            });
        })();
    }, [connection]);

    const useCurrentGame = () => {
        const [currentGame, setCurrentGame] = useState<string>();

        const subscribeAsync = useCallback((async (onStoreChange: () => void) => {
            const currentGameResponse = await fetch(`${API_URL}/api/games/current`);
            
            setCurrentGame(((await currentGameResponse.json()) as GameInfo).id);

            onStoreChange();

            (await connection).on("CurrentGameChanged", (newGameId: string) => {
                setCurrentGame(newGameId);
                onStoreChange();
            });
        }), [connection, setCurrentGame]);

        const subscribe = useCallback((onStoreChange: () => void) => {
            subscribeAsync(onStoreChange);

            return () => {};
        }, [subscribeAsync]);

        return useSyncExternalStore(subscribe, () => currentGame);
    };

    return (
        <SystemStateContext.Provider value={{ useCurrentGame }}>
            { children }
        </SystemStateContext.Provider>
    )
}