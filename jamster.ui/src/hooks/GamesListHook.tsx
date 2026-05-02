import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useState } from "react"
import { useHubConnection } from "./SignalRHubConnection";
import { HubConnection } from "@microsoft/signalr";
import { GameInfo } from "@/types";
import { useGameApi } from "./GameApiHook";
import { v4 as uuidv4 } from 'uuid';

type CallbackHandle = string;

type GamesListChanged = (games: GameInfo[]) => void;
type GamesListWatch = (onGamesListChanged: GamesListChanged) => CallbackHandle;
type GamesListUnwatch = (handle: CallbackHandle) => void;

type GamesListContextProps = {
    watchGamesList: GamesListWatch,
    unwatchGamesList: GamesListUnwatch,
    connection?: HubConnection,
};

const GamesListContext = createContext<GamesListContextProps>({
    watchGamesList: () => { throw new Error('watchGamesList called before context created'); },
    unwatchGamesList: () => { throw new Error('unwatchGamesList called before context created'); },
});

export const useGamesList = () => {
    const context = useContext(GamesListContext);
    const [value, setValue] = useState<GameInfo[]>([]);
    const gameApi = useGameApi();
    
    const getInitialState = useCallback(async () => {
        return await gameApi.getGames();
    }, []);

    useEffect(() => {
        getInitialState().then(setValue);
    }, []);
    
    useEffect(() => {
        const handle = context.watchGamesList(setValue);

        return () => context.unwatchGamesList(handle);
    }, []);

    return value;
}

export const GamesListContextProvider = ({ children }: PropsWithChildren) => {
    const [gamesListNotifiers, setGamesListNotifiers] = useState<Record<CallbackHandle, GamesListChanged>>({});

    const { connection } = useHubConnection('games');

    const watchGamesList = (onStateChange: GamesListChanged) => {
        const newId = uuidv4();

        setGamesListNotifiers(notifiers => ({
            ...notifiers,
            [newId]: onStateChange,
        }));

        return newId;
    }

    const unwatchGamesList = (handle: CallbackHandle) => {
        setGamesListNotifiers(notifiers => {
            if(!notifiers[handle]) {
                console.warn("Attempt to unwatch games list change with invalid handle", handle);
                return notifiers;
            }

            // eslint-disable-next-line @typescript-eslint/no-unused-vars
            const { [handle]: _, ...newNotifiers } = notifiers;

            return newNotifiers;
        })
    }

    useEffect(() => {
        connection?.invoke("WatchGamesList");
    }, [connection]);

    useEffect(() => {
        connection?.onreconnected(() => {
            connection?.invoke("WatchGamesList");
        });
    }, [connection]);

    const notify = useCallback((games: GameInfo[]) => {
        Object.values(gamesListNotifiers).forEach(n => n(games));
    }, [gamesListNotifiers]);

    useEffect(() => {
        connection?.on("GamesListChanged", (games: GameInfo[]) => {
            notify(games);
        });

        return () => connection?.off("GamesListChanged");
    }, [connection, notify]);

    return (
        <GamesListContext.Provider value={{ watchGamesList, unwatchGamesList, connection  }}>
            { children }
        </GamesListContext.Provider>
    )
}