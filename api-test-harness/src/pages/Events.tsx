import { Button } from "@/components/ui/button";
import { useCallback, useEffect, useMemo, useState } from "react";
import * as SignalR from '@microsoft/signalr';
import { ComboBox } from "@/components/ui/combobox";
import { Plus } from "lucide-react";

const API_URL = 'https://localhost:7255';
//const API_URL = 'http://localhost:5000';

type GameModel = {
    id: string,
    name: string,
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
    jamNumber: number,
    startTick: number,
    ticksPassed: number,
    secondsPassed: number,
};

type LineupClockState = {
    isRunning: boolean,
    jamNumber: number,
    startTick: number,
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

function Clock<TClockState>({ gameId, secondsMapper, stateName, direction, startValue }: ClockProps<TClockState>) {

    const [clock, setClock] = useState<number>(startValue ?? 0)

    useEffect(() => {
        if(!gameId) {
            return;
        }

        const hubConnection = new SignalR.HubConnectionBuilder()
            .withUrl(`${API_URL}/api/hubs/game/${gameId}`, { withCredentials: false })
            .build();

        hubConnection.on("StateChanged", (state: TClockState) => {
            setClock(secondsMapper(state));
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

    const startJam = useCallback(async () => {
        await sendEvent("JamStarted");
    }, [sendEvent]);

    const endJam = useCallback(async () => {
        await sendEvent("JamEnded");
    }, [sendEvent]);

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
            Jam: <JamClock gameId={gameId} />
            Lineup: <LineupClock gameId={gameId} />
            <Button onClick={startJam} className="m-[2px]">Start Jam</Button>
            <Button onClick={endJam} className="m-[2px]">End Jam</Button>
        </>  
    );
}