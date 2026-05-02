import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useRef, useState } from "react"
import { useHubConnection } from "./SignalRHubConnection";
import { HubConnection } from "@microsoft/signalr";
import { GameInfo } from "@/types";
import { useGameApi } from "./GameApiHook";
import { v4 as uuidv4 } from 'uuid';

type CallbackHandle = string;

type CurrentGameChanged = (games: GameInfo) => void;
type CurrentGameWatch = (onCurrentGameChanged: CurrentGameChanged) => CallbackHandle;
type CurrentGameUnwatch = (handle: CallbackHandle) => void;

type SystemStateContextProps = {
    watchCurrentGame: CurrentGameWatch,
    unwatchCurrentGame: CurrentGameUnwatch,
    connection?: HubConnection,
};

const SystemStateContext = createContext<SystemStateContextProps>({
    watchCurrentGame: () => { throw new Error('watchCurrentGame called before context created'); },
    unwatchCurrentGame: () => { throw new Error('unwatchCurrentGame called before context created'); },
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
        const callbackHandle = context.watchCurrentGame(game => {
            console.debug("Current game changed", game);
            setCurrentGame(game);
        });

        return () => {
            context.unwatchCurrentGame(callbackHandle);
        }
    }, [context.connection, setCurrentGame]);

    const updateCurrentGame = async (gameId: string) => {
        await gameApi.setCurrentGame(gameId);
    };

    return { currentGame, setCurrentGame: updateCurrentGame };
}

export const SystemStateContextProvider = ({ children }: PropsWithChildren) => {
    const [currentGameNotifiers, setCurrentGameNotifiers] = useState<Record<CallbackHandle, CurrentGameChanged>>({});

    const { connection } = useHubConnection(`System`);

    const watchCurrentGame = (onStateChange: CurrentGameChanged) => {

        const newId = uuidv4();

        setCurrentGameNotifiers(notifiers => ({
            ...notifiers,
            [newId]: onStateChange
        }));

        return newId;
    }

    const unwatchCurrentGame = (handle: CallbackHandle) => {
        setCurrentGameNotifiers(cgn => {
            if (!cgn[handle]) {
                console.warn("Attempt to unwatch current game with invalid handle");
            }

            // eslint-disable-next-line @typescript-eslint/no-unused-vars
            const { [handle]: _, ...newNotifiers } = cgn;

            return newNotifiers;
        })
    }

    useEffect(() => {
        if(!connection) {
            return;
        }

        connection?.invoke("WatchSystemState");
    }, [connection]);

    const currentGameNotifiersRef = useRef(currentGameNotifiers);
    currentGameNotifiersRef.current = currentGameNotifiers;

    useEffect(() => {
        if (!connection) {
            return;
        }

        connection?.onreconnected(() => {
            connection?.invoke("WatchSystemState");
        });
    }, [connection]);

    useEffect(() => {
        connection?.on("CurrentGameChanged", (game: GameInfo) => {
            Object.values(currentGameNotifiersRef.current).forEach(n => n(game));
        });

        return () => connection?.off("CurrentGameChanged");
    }, [connection]);

    return (
        <SystemStateContext.Provider value={{ watchCurrentGame, unwatchCurrentGame, connection  }}>
            { children }
        </SystemStateContext.Provider>
    )
}