import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useState } from "react"
import { useHubConnection } from "./SignalRHubConnection";
import { HubConnection } from "@microsoft/signalr";
import { useGameApi } from "./GameApiHook";
import { GameStageState, TripScoreState, TeamDetailsState, TeamScoreState, TeamSide, TeamTimeoutsState, JamLineupState } from "@/types";
import { CurrentTimeoutTypeState } from "@/types/CurrentTimeoutTypeState";

type StateChanged<TState> = (state: TState) => void;
type StateWatch = <TState,>(stateName: string, onStateChange: StateChanged<TState>) => void;

type GameStateContextProps = {
    gameId?: string,
    stateNotifiers: StateNotifierMap,
    watchState: StateWatch,
    connection?: HubConnection,
};

const GameStateContext = createContext<GameStateContextProps>({
    stateNotifiers: {},
    watchState: () => { throw new Error('watchState called before context created'); },
});

type GameStateContextProviderProps = {
    gameId: string | undefined,
};

type StateNotifier = (genericState: object) => void;
type StateNotifierMap = { [key: string]: StateNotifier[] };

export const useCurrentTimeoutTypeState = () => useGameState<CurrentTimeoutTypeState>("CurrentTimeoutTypeState");
export const useGameStageState = () => useGameState<GameStageState>("GameStageState");
export const useJamLineupState = (side: TeamSide) => useGameState<JamLineupState>(`JamLineupState_${TeamSide[side]}`);
export const useTeamDetailsState = (side: TeamSide) => useGameState<TeamDetailsState>(`TeamDetailsState_${TeamSide[side]}`);
export const useTeamScoreState = (side: TeamSide) => useGameState<TeamScoreState>(`TeamScoreState_${TeamSide[side]}`);
export const useTeamTimeoutsState = (side: TeamSide) => useGameState<TeamTimeoutsState>(`TeamTimeoutsState_${TeamSide[side]}`);
export const useTripScoreState = (side: TeamSide) => useGameState<TripScoreState>(`TripScoreState_${TeamSide[side]}`);

export const useGameState = <TState,>(stateName: string) => {
    const context = useContext(GameStateContext);
    const [value, setValue] = useState<TState>();
    const gameApi = useGameApi();
    
    const getInitialState = useCallback(async (stateName: string) => {
        if(context.gameId) {
            return await gameApi.getGameState<TState>(context.gameId, stateName);
        }
        return undefined;
    }, [context.gameId]);

    useEffect(() => {
        getInitialState(stateName).then(setValue);
    }, [context.gameId]);
    
    useEffect(() => {
        context.watchState<TState>(stateName, setValue);
    }, []);

    return value;
}

export const GameStateContextProvider = ({ gameId, children }: PropsWithChildren<GameStateContextProviderProps>) => {
    const [stateNotifiers, setStateNotifiers] = useState<StateNotifierMap>({});

    const connection = useHubConnection(`game/${gameId}`);

    const watchState = <TState,>(stateName: string, onStateChange: StateChanged<TState>) => {
        setStateNotifiers(sn => ({
            ...sn,
            [stateName]: [
                ...(sn[stateName] ?? []),
                genericState => onStateChange(genericState as TState)
            ]
        }));
    }

    useEffect(() => {
        if(!connection) {
            return;
        }

        Object.keys(stateNotifiers).forEach(stateName => {
            connection?.invoke("WatchState", stateName);
        });
    }, [connection, stateNotifiers]);

    useEffect(() => {
        (async () => {
            connection?.onreconnected(() => {
                Object.keys(stateNotifiers).forEach(stateName => connection?.invoke("WatchState", stateName));
            });
        })();
    }, [connection, stateNotifiers]);

    useEffect(() => {
        connection?.on("StateChanged", (stateName: string, state: object) => {
            stateNotifiers[stateName]?.forEach(n => n(state));
        });
    }, [connection]);

    return (
        <GameStateContext.Provider value={{ gameId, stateNotifiers, watchState, connection  }}>
            { children }
        </GameStateContext.Provider>
    )
}