import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useRef, useState } from "react"
import { useHubConnection } from "./SignalRHubConnection";
import { GameInfo } from "@/types";
import { gameApi } from "./GameApi";
import { v4 as uuidv4 } from 'uuid';

type CallbackHandle = string;

type GamesListChanged = (games: GameInfo[]) => void;
type GamesListWatch = (onGamesListChanged: GamesListChanged) => CallbackHandle;
type GamesListUnwatch = (handle: CallbackHandle) => void;

type GamesListContextProps = {
    watchGamesList: GamesListWatch,
    unwatchGamesList: GamesListUnwatch,
};

type GamesListNotifierMap = Record<CallbackHandle, GamesListChanged>;

const GamesListContext = createContext<GamesListContextProps>({
    watchGamesList: () => { throw new Error('watchGamesList called before context created'); },
    unwatchGamesList: () => { throw new Error('unwatchGamesList called before context created'); },
});

export const useGamesList = () => {
    const context = useContext(GamesListContext);
    const [value, setValue] = useState<GameInfo[]>([]);
    
    const getInitialState = useCallback(async () => {
        return await gameApi.getGames();
    }, []);

    useEffect(() => {
        getInitialState().then(setValue);
    }, []);
    
    useEffect(() => {
        const handle = context.watchGamesList(setValue);

        return () => context.unwatchGamesList(handle);
    }, [context.watchGamesList, context.unwatchGamesList]);

    return value;
}

export const GamesListContextProvider = ({ children }: PropsWithChildren) => {
    const gamesListNotifiersRef = useRef<GamesListNotifierMap>({});

    const { connection } = useHubConnection('games');

    useEffect(() => {
        if(!connection) return;

        connection.invoke("WatchGamesList");
        connection.onreconnected(() => {
            connection?.invoke("WatchGamesList");
        });
    }, [connection]);

    const watchGamesList = useCallback((onStateChange: GamesListChanged) => {
        const newId = uuidv4();

        gamesListNotifiersRef.current[newId] = onStateChange;

        return newId;
    }, []);

    const unwatchGamesList = useCallback((handle: CallbackHandle) => {
        if(!gamesListNotifiersRef.current[handle]) {
            console.warn("Attempt to unwatch games list change with invalid handle", handle);
            return;
        }

        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        const { [handle]: _, ...newNotifiers } = gamesListNotifiersRef.current;

        gamesListNotifiersRef.current = newNotifiers;
    }, []);

    const notify = useCallback((games: GameInfo[]) => {
        Object.values(gamesListNotifiersRef.current).forEach(n => n(games));
    }, []);

    useEffect(() => {
        connection?.on("GamesListChanged", (games: GameInfo[]) => {
            notify(games);
        });

        return () => connection?.off("GamesListChanged");
    }, [connection, notify]);

    const context = useMemo(
        () => ({ watchGamesList, unwatchGamesList  }),
        [watchGamesList, unwatchGamesList]
    );

    return (
        <GamesListContext.Provider value={context}>
            { children }
        </GamesListContext.Provider>
    )
}