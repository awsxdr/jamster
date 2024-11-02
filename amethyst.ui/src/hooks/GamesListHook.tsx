import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useState } from "react"
import { API_URL, useHubConnection } from "./SignalRHubConnection";
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

    const connection = useHubConnection(`games`);

    const watchGamesList = (onStateChange: GamesListChanged) => {
        setGamesListNotifiers(notifiers => [
            ...notifiers,
            onStateChange
        ]);
    }

    useEffect(() => {
        if(!connection) {
            return;
        }

        gamesListNotifiers.forEach(() => {
            connection?.invoke("WatchGamesList");
        });
    }, [connection, gamesListNotifiers]);

    useEffect(() => {
        (async () => {
            connection?.onreconnected(() => {
                gamesListNotifiers.forEach(() => connection?.invoke("WatchGamesList"));
            });
        })();
    }, [connection, gamesListNotifiers]);

    useEffect(() => {
        connection?.on("StateChanged", (games: GameInfo[]) => {
            gamesListNotifiers.forEach(n => n(games));
        });
    }, [connection]);

    return (
        <GamesListContext.Provider value={{ gamesListNotifiers, watchGamesList, connection  }}>
            { children }
        </GamesListContext.Provider>
    )
}