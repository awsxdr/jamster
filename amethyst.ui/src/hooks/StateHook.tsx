import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useState, useSyncExternalStore } from "react"
import * as SignalR from '@microsoft/signalr';

type StateWatch = <TState,>(stateName: string) => TState | undefined;

type GameStateContextProps = {
    useStateWatch: StateWatch,
};

const GameStateContext = createContext<GameStateContextProps>({
    useStateWatch: () => { throw new Error("watchState used before context created"); },
});

export const useGameState = () => useContext(GameStateContext);

type GameStateContextProviderProps = {
    gameId: string,
};

const API_URL = 'https://localhost:7255';

export const GameStateContextProvider = ({ gameId, children }: PropsWithChildren<GameStateContextProviderProps>) => {

    const [connection, setConnection] = useState<Promise<SignalR.HubConnection>>(Promise.reject("Connection not yet started"));
    const [watchedStates, setWatchedStates] = useState<string[]>([]);
    
    useEffect(() => {
        if(!gameId) {
            return;
        }

        const hubConnection = new SignalR.HubConnectionBuilder()
            .withUrl(`${API_URL}/api/hubs/game/${gameId}`, { withCredentials: false })
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
    }, [gameId, setConnection]);

    const useStateWatch = <TState,>(stateName: string) => {

        const [state, setState] = useState<TState>();

        const subscribeAsync = useCallback((async (onStoreChange: () => void) => {
            const currentStateResponse = await fetch(`${API_URL}/api/games/${gameId}/state/${stateName}`);
            const currentState = (await currentStateResponse.json()) as TState;

            setState(currentState);
            
            onStoreChange();

            (await connection).on("StateChanged", (changedStateName: string, genericState: object) => {
                if(changedStateName === stateName) {
                    const newState = genericState as TState;
                    setState(newState);
                    onStoreChange();
                }
            });

            if(!watchedStates.find(s => s === stateName)) {
                setWatchedStates([...watchedStates, stateName]);
                (await connection).invoke("WatchState", stateName);
            }
        }), [connection, stateName]);

        const subscribe = useCallback((onStoreChange: () => void) => {
            subscribeAsync(onStoreChange);

            return () => {};
        }, [subscribeAsync]);

        return useSyncExternalStore(
            subscribe,
            () => state);
    };

    return (
        <GameStateContext.Provider value={{ useStateWatch }}>
            { children }
        </GameStateContext.Provider>
    )
}