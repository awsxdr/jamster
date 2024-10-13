import { Button } from "@/components/ui/button";
import { useCallback, useEffect, useMemo, useState } from "react";
import * as SignalR from '@microsoft/signalr';
import { ComboBox } from "@/components/ui/combobox";
import { Plus } from "lucide-react";

const API_URL = 'https://localhost:7255';
//const API_URL = 'http://localhost:5000';
//const API_URL = 'http://localhost:5249';

type GameModel = {
    id: string,
    name: string,
};

type ScoreModifiedRelativeBody = {
    teamSide: string,
    value: number,
};

type ClockProps<TClockState> = {
    gameId?: string,
    secondsMapper: (state: TClockState) => number,
    stateName: string,
    direction: "down" | "up",
    startValue?: number,
};

type JamClockState = {
    isRunning: boolean,
    startTick: number,
    ticksPassed: number,
    secondsPassed: number,
};

type LineupClockState = {
    isRunning: boolean,
    startTick: number,
    ticksPassed: number,
    secondsPassed: number,
};

type TimeoutClockState = {
    isRunning: boolean,
    startTick: number,
    ticksPassed: number,
    secondsPassed: number,
};

type PeriodClockState = {
    isRunning: boolean,
    lastStartTick: number,
    ticksPassedAtLastStart: number,
    ticksPassed: number,
    secondsPassed: number,
};

type BasicClockProps<TClockState> = Pick<ClockProps<TClockState>, "gameId">;

const JamClock = ({ gameId }: BasicClockProps<JamClockState>) => (
    <Clock<JamClockState> gameId={gameId} secondsMapper={s => s.secondsPassed} stateName="JamClockState" direction="down" startValue={120} />
);

const LineupClock = ({ gameId }: BasicClockProps<LineupClockState>) => (
    <Clock<LineupClockState> gameId={gameId} secondsMapper={s => s.secondsPassed} stateName="LineupClockState" direction="up" />
);

const TimeoutClock = ({ gameId }: BasicClockProps<TimeoutClockState>) => (
    <Clock<TimeoutClockState> gameId={gameId} secondsMapper={s => s.secondsPassed} stateName="TimeoutClockState" direction="up" />
);

const PeriodClock = ({ gameId }: BasicClockProps<PeriodClockState>) => (
    <Clock<PeriodClockState> gameId={gameId} secondsMapper={s => s.secondsPassed} stateName="PeriodClockState" direction="down" startValue={30 * 60} />
);

const Clock = <TClockState,>({ gameId, secondsMapper, stateName, direction, startValue }: ClockProps<TClockState>) => {
    const [clock, setClock] = useState<number>(startValue ?? 0)

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

        hubConnection.on("StateChanged", (_changedStateName: string, state: TClockState) => {
            setClock(secondsMapper(state));
        });

        hubConnection.onreconnected(() => {
            hubConnection.invoke("WatchState", stateName);
        });

        hubConnection.start()
            .catch(err => console.error(err))
            .then(() => {
                hubConnection.invoke("WatchState", stateName);
            });

        return () => {
            hubConnection.stop();
        }
        
    }, [gameId, setClock, secondsMapper, stateName]);

    const time = useMemo(() => {
        const totalSeconds = direction === 'up' ? clock : ((startValue ?? 0) - clock);
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = totalSeconds % 60;

        return minutes > 0 ? `${minutes}:${seconds.toString().padStart(2, '0')}` : `${seconds}`;
    }, [clock, direction, startValue]);

    return (
        <div>
            {time}
        </div>
    );
};

type GameStageDisplayProps = {
    gameId?: string
};

enum Stage {
    BeforeGame,
    Lineup,
    Jam,
    Timeout,
    Intermission,
    AfterGame,
}

type GameStageState = {
    stage: Stage,
    periodNumber: number,
    jamNumber: number,
};

const GameStageDisplay = ({gameId}: GameStageDisplayProps) => {

    const [state, setState] = useState<GameStageState>({ stage: Stage.BeforeGame, periodNumber: 0, jamNumber: 0});

    useEffect(() => {
        if(!gameId) {
            return;
        }

        (async () => {
            const currentStateResponse = await fetch(`${API_URL}/api/games/${gameId}/state/GameStageState`);
            const currentState = (await currentStateResponse.json()) as GameStageState;
            setState(currentState);
        })();

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

        hubConnection.on("StateChanged", (_changedStateName: string, state: GameStageState) => {
            setState(state);
        });

        hubConnection.onreconnected(() => {
            hubConnection.invoke("WatchState", "GameStageState");
        });

        hubConnection.start()
            .catch(err => console.error(err))
            .then(() => {
                hubConnection.invoke("WatchState", "GameStageState");
            });

        return () => {
            hubConnection.stop();
        }
        
    }, [gameId, setState]);

    return (
        <div>
            <p>Stage: {state.stage}</p>
            <p>Period: {state.periodNumber}</p>
            <p>Jam: {state.jamNumber}</p>
        </div>
    );
}

export const Events = () => {

    const [gameId, setGameId] = useState<string>();
    const [games, setGames] = useState<GameModel[]>([]);

    const getGames = useCallback(async () => {
        const gamesResponse = await fetch(`${API_URL}/api/Games`);
        setGames((await gamesResponse.json()) as GameModel[]);
    }, [setGames]);

    useEffect(() => {
        getGames?.();
    }, [getGames]);

    const createNewGame = useCallback(async () => {
        const response = await fetch(`${API_URL}/api/Games`, {
            method: 'POST',
            body: JSON.stringify({
                name: "Test game",
            }),
            headers: {
                "Content-type": "application/json; charset=utf-8",
            }
        });

        if(!response.ok) {
            alert('request failed');
            return;
        }

        await getGames();

        const game: GameModel = await response.json();
        setGameId(game.id);
    }, [setGameId, getGames]);

    const sendEvent = useCallback(async (eventName: string) => {
        await fetch(`${API_URL}/api/Games/${gameId}/events`, {
            method: 'POST',
            body: JSON.stringify({
                type: eventName,
            }),
            headers: {
                "Content-type": "application/json; charset=utf-8",
            }
        });
    }, [gameId]);

    const sendEventWithBody = useCallback(async <TBody,>(eventName: string, body: TBody) => {
        await fetch(`${API_URL}/api/Games/${gameId}/events`, {
            method: 'POST',
            body: JSON.stringify({
                type: eventName,
                body: body
            }),
            headers: {
                "Content-type": "application/json; charset=utf-8",
            }
        });
    }, [gameId]);

    const endIntermission = useCallback(async () => {
        await sendEvent("IntermissionEnded");
    }, [sendEvent]);

    const finalizePeriod = useCallback(async () => {
        await sendEvent("PeriodFinalized");
    }, [sendEvent]);

    const startJam = useCallback(async () => {
        await sendEvent("JamStarted");
    }, [sendEvent]);

    const endJam = useCallback(async () => {
        await sendEvent("JamEnded");
    }, [sendEvent]);

    const startTimeout = useCallback(async () => {
        await sendEvent("TimeoutStarted");
    }, [sendEvent]);

    const endTimeout = useCallback(async () => {
        await sendEvent("TimeoutEnded");
    }, [sendEvent]);

    const addPoint = useCallback((team: string, value: number) => async () => {
        await sendEventWithBody<ScoreModifiedRelativeBody>("ScoreModifiedRelative", { teamSide: team, value });
    }, [sendEventWithBody]);

    return (
        <>
            <div>
                <ComboBox 
                    items={games.map(game => ({ value: game.id, text: `${game.name} (${game.id})`}))}
                    value={gameId ?? ""}
                    placeholder="Select game..."
                    onValueChanged={setGameId}
                />
                <Button variant="outline" onClick={() => createNewGame()}>
                    <Plus className="h-4 w-4" />
                </Button>
            </div>
            <GameStageDisplay gameId={gameId} />
            Jam: <JamClock gameId={gameId} />
            Lineup: <LineupClock gameId={gameId} />
            Timeout: <TimeoutClock gameId={gameId} />
            Period: <PeriodClock gameId={gameId} />
            <div>
                <Button onClick={endIntermission} className="m-[2px]">End intermission</Button>
                <Button onClick={finalizePeriod} className="m-[2px]">Finalize period</Button>
            </div>
            <div>
                <Button onClick={startJam} className="m-[2px]">Start Jam</Button>
                <Button onClick={endJam} className="m-[2px]">End Jam</Button>
            </div>
            <div>
                <Button onClick={startTimeout} className="m-[2px]">Start Timeout</Button>
                <Button onClick={endTimeout} className="m-[2px]">End Timeout</Button>
            </div>
            <div>
                Home 
                <Button onClick={addPoint("Home", 1)} className="m-[2px]">+1</Button>
                <Button onClick={addPoint("Home", -1)} className="m-[2px]">-1</Button>
            </div>
            <div>
                Away 
                <Button onClick={addPoint("Away", 1)} className="m-[2px]">+1</Button>
                <Button onClick={addPoint("Away", -1)} className="m-[2px]">-1</Button>
            </div>
        </>  
    );
}