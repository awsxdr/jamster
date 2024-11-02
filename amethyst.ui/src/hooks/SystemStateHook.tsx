import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useState, useSyncExternalStore } from "react"
import { API_URL, useHubConnection } from "./SignalRHubConnection";

type SystemStateContextProps = {
    useCurrentGame: () => string | undefined,
};

const SystemStateContext = createContext<SystemStateContextProps>({
    useCurrentGame: () => { throw new Error("useCurrentGame used before context created"); },
});

export const useSystemState = () => useContext(SystemStateContext);

type GameInfo = {
    id: string,
    name: string,
};

export const SystemStateContextProvider = ({ children }: PropsWithChildren) => {

    const connection = useHubConnection("system");

    useEffect(() => {
        (async () => {
            connection?.onreconnected(() => {
                connection?.invoke("WatchSystemState");
            });
        })();
    }, [connection]);

    const useCurrentGame = () => {
        const [currentGame, setCurrentGame] = useState<string>();

        const subscribeAsync = useCallback((async (onStoreChange: () => void) => {
            const currentGameResponse = await fetch(`${API_URL}/api/games/current`);
            
            setCurrentGame(((await currentGameResponse.json()) as GameInfo).id);

            onStoreChange();

            connection?.on("CurrentGameChanged", (newGameId: string) => {
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