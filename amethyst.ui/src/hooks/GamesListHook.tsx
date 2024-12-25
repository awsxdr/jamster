import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useState } from "react"
import { useHubConnection } from "./SignalRHubConnection";
import { HubConnection } from "@microsoft/signalr";
import { GameInfo } from "@/types";
import { useGameApi } from "./GameApiHook";

type GamesListChanged = (games: GameInfo[]) => void;
type GamesListWatch = (onGamesListChanged: GamesListChanged) => void;

type GamesListContextProps = {
    gamesListNotifiers: GamesListChanged[],
    watchGamesList: GamesListWatch,
    connection?: HubConnection,
};

const GamesListContext = createContext<GamesListContextProps>({
    gamesListNotifiers: [],
    watchGamesList: () => { throw new Error('watchGamesList called before context created'); },
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
        context.watchGamesList(setValue);
    }, []);

    return value;
}

export const GamesListContextProvider = ({ children }: PropsWithChildren) => {
    const [gamesListNotifiers, setGamesListNotifiers] = useState<GamesListChanged[]>([]);

    const { connection } = useHubConnection('games');

    const watchGamesList = (onStateChange: GamesListChanged) => {
        setGamesListNotifiers(notifiers => [
            ...notifiers,
            onStateChange
        ]);
    }

    useEffect(() => {
        connection?.invoke("WatchGamesList");
    }, [connection]);

    useEffect(() => {
        (async () => {
            connection?.onreconnected(() => {
                connection?.invoke("WatchGamesList");
            });
        })();
    }, [connection]);

    const notify = useCallback((games: GameInfo[]) => {
        gamesListNotifiers.forEach(n => n(games));
    }, [gamesListNotifiers]);

    useEffect(() => {
        connection?.on("GamesListChanged", (games: GameInfo[]) => {
            notify(games);
        });
    }, [connection, notify]);

    return (
        <GamesListContext.Provider value={{ gamesListNotifiers, watchGamesList, connection  }}>
            { children }
        </GamesListContext.Provider>
    )
}