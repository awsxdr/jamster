import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useState } from "react"
import { useHubConnection } from "./SignalRHubConnection";
import { HubConnection } from "@microsoft/signalr";
import { GameInfo } from "@/types";
import { useGameApi } from "./GameApiHook";

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
    const gameApi = useGameApi();
    
    const getInitialState = useCallback(async () => {
        return await gameApi.getCurrentGame();
    }, []);

    useEffect(() => {
        getInitialState().then(setCurrentGame);
    }, [getInitialState, setCurrentGame]);
    
    useEffect(() => {
        context.watchCurrentGame(game => {
            console.debug("Current game changed", game);
            setCurrentGame(game);
        });
    }, [context.connection, setCurrentGame]);

    const updateCurrentGame = async (gameId: string) => {
        await gameApi.setCurrentGame(gameId);
    };

    return { currentGame, setCurrentGame: updateCurrentGame };
}

export const SystemStateContextProvider = ({ children }: PropsWithChildren) => {
    const [currentGameNotifiers, setCurrentGameNotifiers] = useState<CurrentGameChanged[]>([]);

    const { connection } = useHubConnection(`System`);

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