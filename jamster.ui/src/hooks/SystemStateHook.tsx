import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useRef, useState } from "react"
import { useHubConnection } from "./SignalRHubConnection";
import { GameInfo } from "@/types";
import { useGameApi } from "./GameApiHook";
import { v4 as uuidv4 } from 'uuid';

type CallbackHandle = string;

type CurrentGameChanged = (games: GameInfo) => void;
type CurrentGameWatch = (onCurrentGameChanged: CurrentGameChanged) => CallbackHandle;
type CurrentGameUnwatch = (handle: CallbackHandle) => void;

type CurrentGameNotifierMap = Record<CallbackHandle, CurrentGameChanged>;

type SystemStateContextProps = {
    watchCurrentGame: CurrentGameWatch,
    unwatchCurrentGame: CurrentGameUnwatch,
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
    }, [gameApi]);

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
    }, [context.watchCurrentGame, context.unwatchCurrentGame, setCurrentGame]);

    const updateCurrentGame = async (gameId: string) => {
        await gameApi.setCurrentGame(gameId);
    };

    return { currentGame, setCurrentGame: updateCurrentGame };
}

export const SystemStateContextProvider = ({ children }: PropsWithChildren) => {
    const { connection } = useHubConnection(`System`);

    const currentGameNotifiersRef = useRef<CurrentGameNotifierMap>({});

    useEffect(() => {
        if (!connection) {
            return;
        }

        connection?.invoke("WatchSystemState");
        connection?.onreconnected(() => {
            connection?.invoke("WatchSystemState");
        });
    }, [connection]);

    const watchCurrentGame = useCallback((onStateChange: CurrentGameChanged) => {
        const newId = uuidv4();

        currentGameNotifiersRef.current[newId] = onStateChange;

        return newId;
    }, []);

    const unwatchCurrentGame = useCallback((handle: CallbackHandle) => {
        if (!currentGameNotifiersRef.current[handle]) {
            console.warn("Attempt to unwatch current game with invalid handle");
            return;
        }
        
        // eslint-disable-next-line @typescript-eslint/no-unused-vars
        const { [handle]: _, ...newNotifiers } = currentGameNotifiersRef.current ?? {};

        currentGameNotifiersRef.current = newNotifiers;
    }, []);

    useEffect(() => {
        connection?.on("CurrentGameChanged", (game: GameInfo) => {
            Object.values(currentGameNotifiersRef.current).forEach(n => n(game));
        });

        return () => connection?.off("CurrentGameChanged");
    }, [connection]);

    const context = useMemo(
        () => ({ watchCurrentGame, unwatchCurrentGame }),
        [watchCurrentGame, unwatchCurrentGame]
    );

    return (
        <SystemStateContext.Provider value={context}>
            { children }
        </SystemStateContext.Provider>
    )
}