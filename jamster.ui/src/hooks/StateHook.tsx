import { createContext, PropsWithChildren, useCallback, useContext, useEffect, useMemo, useState } from "react"
import { useHubConnection } from "./SignalRHubConnection";
import { HubConnection } from "@microsoft/signalr";
import { useGameApi } from "./GameApiHook";
import { GameStageState, TripScoreState, TeamDetailsState, TeamScoreState, TeamSide, TeamTimeoutsState, JamLineupState, TimeoutListState, PeriodClockState, TimeoutClockState, JamClockState, LineupClockState, UndoListState, ScoreSheetState, RulesState, LineupSheetState, PenaltyBoxState, GameSummaryState, PenaltySheetState, InjuriesState, StringMap, BoxTripsState, TimelineState } from "@/types";
import { CurrentTimeoutTypeState } from "@/types/CurrentTimeoutTypeState";
import { TeamJamStatsState } from "@/types/TeamJamStatsState";
import { v4 as uuidv4 } from 'uuid';
import { IntermissionClockState } from "@/types/IntermissionClockState";

type StateChanged<TState> = (state: TState) => void;
type StateWatch = <TState,>(stateName: string, onStateChange: StateChanged<TState>) => CallbackHandle;

type GameStateContextProps = {
    gameId?: string;
    stateNotifiers: StateNotifierMap;
    watchState: StateWatch;
    unwatchState: (stateName: string, handle: CallbackHandle) => void;
    connection?: HubConnection;
    hasConnection: boolean;
};

const GameStateContext = createContext<GameStateContextProps>({
    stateNotifiers: {},
    watchState: () => { throw new Error('watchState called before context created'); },
    unwatchState: () => { throw new Error('unwatchState called before context created'); },
    hasConnection: false,
});

type GameStateContextProviderProps = {
    gameId: string,
};

type StateNotifier = Record<CallbackHandle, (genericState: object) => void>;
type StateNotifierMap = Record<string, StateNotifier>;

export const useCurrentTimeoutTypeState = () => useGameState<CurrentTimeoutTypeState>("CurrentTimeoutTypeState");
export const useGameStageState = () => useGameState<GameStageState>("GameStageState");
export const useJamLineupState = (side: TeamSide) => useGameState<JamLineupState>(`JamLineupState_${TeamSide[side]}`);
export const useJamStatsState = (side: TeamSide) => useGameState<TeamJamStatsState>(`TeamJamStatsState_${TeamSide[side]}`);
export const useTeamDetailsState = (side: TeamSide) => useGameState<TeamDetailsState>(`TeamDetailsState_${TeamSide[side]}`);
export const useTeamScoreState = (side: TeamSide) => useGameState<TeamScoreState>(`TeamScoreState_${TeamSide[side]}`);
export const useTeamTimeoutsState = (side: TeamSide) => useGameState<TeamTimeoutsState>(`TeamTimeoutsState_${TeamSide[side]}`);
export const useTripScoreState = (side: TeamSide) => useGameState<TripScoreState>(`TripScoreState_${TeamSide[side]}`);
export const useTimeoutListState = () => useGameState<TimeoutListState>("TimeoutListState");
export const usePeriodClockState = () => useGameState<PeriodClockState>("PeriodClockState");
export const useJamClockState = () => useGameState<JamClockState>("JamClockState");
export const useLineupClockState = () => useGameState<LineupClockState>("LineupClockState");
export const useTimeoutClockState = () => useGameState<TimeoutClockState>("TimeoutClockState");
export const useIntermissionClockState = () => useGameState<IntermissionClockState>("IntermissionClockState");
export const useInjuriesState = (side: TeamSide) => useGameState<InjuriesState>(`InjuriesState_${TeamSide[side]}`);
export const useClocks = () => ({
    periodClock: usePeriodClockState(),
    jamClock: useJamClockState(),
    lineupClock: useLineupClockState(),
    timeoutClock: useTimeoutClockState(),
    intermissionClock: useIntermissionClockState(),
});
export const useUndoListState = () => useGameState<UndoListState>("UndoListState");
export const useScoreSheetState = (side: TeamSide) => useGameState<ScoreSheetState>(`ScoreSheetState_${TeamSide[side]}`);
export const useLineupSheetState = (side: TeamSide) => useGameState<LineupSheetState>(`LineupSheetState_${TeamSide[side]}`);
export const usePenaltyBoxState = (side: TeamSide) => useGameState<PenaltyBoxState>(`PenaltyBoxState_${TeamSide[side]}`);
export const usePenaltySheetState = (side: TeamSide) => useGameState<PenaltySheetState>(`PenaltySheetState_${TeamSide[side]}`);
export const useGameSummaryState = () => useGameState<GameSummaryState>("GameSummaryState");
export const useRulesState = () => useGameState<RulesState>("RulesState");
export const useBoxTripsState = (side: TeamSide) => useGameState<BoxTripsState>(`BoxTripsState_${TeamSide[side]}`);
export const useTimelineState = () => useGameState<TimelineState>("TimelineState");

export const useGameState = <TState,>(stateName: string) => {
    const context = useContext(GameStateContext);
    const [value, setValue] = useState<TState>();

    useEffect(() => {
        const handle = context.watchState<TState>(stateName, setValue);

        return () => context.unwatchState(stateName, handle);
    }, [context.gameId, stateName, setValue]);

    return value;
}

export const useHasServerConnection = () => {
    const { hasConnection } = useContext(GameStateContext);

    return useMemo(() => hasConnection, [hasConnection]);
}

type CallbackHandle = string;

export const GameStateContextProvider = ({ gameId, children }: PropsWithChildren<GameStateContextProviderProps>) => {
    const [stateNotifiers, setStateNotifiers] = useState<StateNotifierMap>({});
    const [states, setStates] = useState<StringMap<object>>({});
    const gameApi = useGameApi();

    const getInitialState = useCallback(async <TState,>(stateName: string) => {
        if(!gameId) {
            return undefined;
        }

        const value: TState = await gameApi.getGameState<TState>(gameId, stateName);

        if(!value) {
            return undefined;
        }

        setStates(s => ({ ...s, [stateName]: value }));
        notify(stateName, value);

        return value;
    }, [gameId, states, stateNotifiers]);

    useEffect(() => {
        if(!gameId) {
            return;
        }

        Object.keys(states).forEach(getInitialState);

    }, [gameId]);

    const handleConnectionDisconnect = async () => {
        if(!connection) return;

        Object.keys(stateNotifiers).forEach(stateName => {
            connection.invoke("UnwatchState", stateName);
        });
    }

    const { connection, isConnected } = useHubConnection(gameId && `game/${gameId}`, handleConnectionDisconnect);

    const watchState = useCallback(<TState,>(stateName: string, onStateChange: StateChanged<TState>): CallbackHandle => {
        
        const newId = uuidv4();

        if(Object.keys(states).includes(stateName)) {
            onStateChange(states[stateName] as TState);
        } else {
            getInitialState(stateName).then(v => v && onStateChange(v as TState));
        }

        setStateNotifiers(sn => ({
            ...sn,
            [stateName]: {
                ...(sn[stateName] ?? {}),
                [newId]: genericState => {
                    onStateChange(genericState as TState);
                    setStates(s => ({ ...s, [stateName]: genericState }));
                }
            }
        }));

        return newId;
    }, [states]);

    const unwatchState = (stateName: string, handle: CallbackHandle) => {
        setStateNotifiers(sn => {

            if (!sn[stateName]?.[handle]) {
                console.warn("Attempt to unwatch state with invalid handle", handle);
            }

            // eslint-disable-next-line @typescript-eslint/no-unused-vars
            const { [handle]: _, ...newNotifier } = sn[stateName] ?? {};
            return {
                ...sn,
                [stateName]: newNotifier
            };
        });
    }

    useEffect(() => {
        if(!connection) {
            return;
        }

        Object.keys(stateNotifiers).forEach(stateName => {
            connection.invoke("WatchState", stateName);
        });
    }, [connection, stateNotifiers]);

    useEffect(() => {
        (async () => {
            connection?.onreconnected(() => {
                Object.keys(stateNotifiers).forEach(stateName => connection?.invoke("WatchState", stateName));
            });
        })();
    }, [connection, stateNotifiers]);

    const notify = useCallback((stateName: string, state: object) => {
        if(!stateNotifiers[stateName]) {
            return;
        }
        Object.values(stateNotifiers[stateName])?.forEach(n => n(state));
    }, [gameId, stateNotifiers]);

    useEffect(() => {
        connection?.on("StateChanged", notify);

        return () => connection?.off("StateChanged", notify);
    }, [connection, notify]);

    const hasConnection = isConnected;

    return (
        <GameStateContext.Provider value={{ gameId, stateNotifiers, watchState, unwatchState, connection, hasConnection  }}>
            { children }
        </GameStateContext.Provider>
    )
}