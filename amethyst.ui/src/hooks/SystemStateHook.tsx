import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useState } from "react"
import { API_URL, useHubConnection } from "./SignalRHubConnection";
import { HubConnection } from "@microsoft/signalr";
import { GameInfo } from "@/types";

type CurrentGameChanged = (games: GameInfo) => void;
type CurrentGameWatch = (onCurrentGameChanged: CurrentGameChanged) => void;

type SystemStateContextProps = {
    watchCurrentGame: CurrentGameWatch,
    connection?: HubConnection,
};

const SystemStateContext = createContext<SystemStateContextProps>({
    watchCurrentGame: () => { throw new Error('watchCurrentGame called before context created'); },
});

export const useCurrentGame = () => {
    const context = useContext(SystemStateContext);
    const [currentGame, setCurrentGame] = useState<GameInfo>();
    
    const getInitialState = useCallback(async () => {
        const currentStateResponse = await fetch(`${API_URL}/api/games/current`);
        return (await currentStateResponse.json()) as GameInfo;
    }, []);

    useEffect(() => {
        getInitialState().then(setCurrentGame);
    }, [getInitialState, setCurrentGame]);
    
    useEffect(() => {
        context.watchCurrentGame(game => {
            console.log("Current game changed", game);
            setCurrentGame(game);
        });
    }, [context.connection, setCurrentGame]);

    const updateCurrentGame = async (gameId: string) => {
        await fetch(
            `${API_URL}/api/games/current`, 
            { 
                method: 'PUT', 
                body: JSON.stringify({ gameId }),
                headers: {
                    "Content-Type": "application/json; charset=utf-8"
                }
            });
    };

    return { currentGame, setCurrentGame: updateCurrentGame };
}

export const SystemStateContextProvider = ({ children }: PropsWithChildren) => {
    const [currentGameNotifiers, setCurrentGameNotifiers] = useState<CurrentGameChanged[]>([]);

    const connection = useHubConnection(`System`);

    const watchCurrentGame = (onStateChange: CurrentGameChanged) => {
        setCurrentGameNotifiers(notifiers => [
            ...notifiers,
            onStateChange
        ]);
    }

    useEffect(() => {
        if(!connection) {
            return;
        }

        currentGameNotifiers.forEach(() => {
            connection?.invoke("WatchSystemState");
        });
    }, [connection, currentGameNotifiers]);

    useEffect(() => {
        (async () => {
            connection?.onreconnected(() => {
                currentGameNotifiers.forEach(() => connection?.invoke("WatchSystemState"));
            });
        })();
    }, [connection, currentGameNotifiers]);

    useEffect(() => {
        connection?.on("CurrentGameChanged", (game: GameInfo) => {
            currentGameNotifiers.forEach(n => n(game));
        });
    }, [connection]);

    return (
        <SystemStateContext.Provider value={{ watchCurrentGame, connection  }}>
            { children }
        </SystemStateContext.Provider>
    )
}