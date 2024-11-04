import { GameInfo } from "@/types"
import { API_URL } from "./SignalRHubConnection"

type GameApi = {
    getGames: () => Promise<GameInfo[]>,
    getGame: (gameId: string) => Promise<GameInfo>,
    getCurrentGame: () => Promise<GameInfo>,
    setCurrentGame: (gameId: string) => Promise<void>,
    getGameState: <TState,>(gameId: string, stateName: string) => Promise<TState>,
}

export const useGameApi: () => GameApi = () => {
    const getGames = async () => {
        const gamesResponse = await fetch(`${API_URL}/api/games`);
        return (await gamesResponse.json()) as GameInfo[];
    }

    const getGame = async (gameId: string) => {
        const response = await fetch(`${API_URL}/api/games/${gameId}`);
        return (await response.json()) as GameInfo;
    }

    const getCurrentGame = async () => {
        const response = await fetch(`${API_URL}/api/games/current`);
        return (await response.json()) as GameInfo;
    }

    const setCurrentGame = async (gameId: string) => {
        await fetch(
            `${API_URL}/api/games/current`, 
            { 
                method: 'PUT', 
                body: JSON.stringify({ gameId }),
                headers: {
                    "Content-Type": "application/json; charset=utf-8"
                }
            });
    }

    const getGameState = async <TState,>(gameId: string, stateName: string) => {
        const currentStateResponse = await fetch(`${API_URL}/api/games/${gameId}/state/${stateName}`);
        return (await currentStateResponse.json()) as TState;
    }

    return {
        getGames,
        getGame,
        getCurrentGame,
        setCurrentGame,
        getGameState,
    }
}