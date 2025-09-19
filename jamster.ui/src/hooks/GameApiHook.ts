import { GameInfo } from "@/types"
import { API_URL } from "@/constants";

type GameApi = {
    getGames: () => Promise<GameInfo[]>;
    getGame: (gameId: string) => Promise<GameInfo>;
    downloadStatsbook: (gameId: string) => Promise<void>;
    deleteGame: (gameId: string) => Promise<void>;
    createGame: (name: string) => Promise<string>;
    uploadGame: (statsBookFile: File) => Promise<string>;
    getCurrentGame: () => Promise<GameInfo>;
    setCurrentGame: (gameId: string) => Promise<void>;
    getGameState: <TState,>(gameId: string, stateName: string) => Promise<TState>;
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

    const downloadStatsbook = async (gameId: string) => {
        const gameInfo = await getGame(gameId);

        const requestUrl = `${API_URL}/api/games/${gameId}`;
        const response = await fetch(requestUrl, { headers: { 'Accept': "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } });
        const blobUrl = URL.createObjectURL(await response.blob());

        const downloadLink = document.createElement('a');
        downloadLink.href = blobUrl;
        downloadLink.download = `${gameInfo.name}.xlsx`;
        document.body.appendChild(downloadLink);
        downloadLink.click();
        downloadLink.remove();

        URL.revokeObjectURL(blobUrl);
    }

    const deleteGame = async (gameId: string) => {
        await fetch(
            `${API_URL}/api/games/${gameId}`,
            {
                method: 'DELETE',
            });
    }

    const createGame = async (name: string) => {
        const response = await fetch(
            `${API_URL}/api/games`,
            {
                method: 'POST',
                body: JSON.stringify({ name }),
                headers: {
                    "Content-Type": "application/json; charset=utf-8"
                }
            });
        
        const { id: gameId } = (await response.json()) as { id: string };

        return gameId;
    }

    const uploadGame = async (statsBookFile: File) => {

        const formData = new FormData();
        formData.append('statsBookFile', statsBookFile);

        const response = await fetch(
            `${API_URL}/api/games`,
            {
                method: 'POST',
                body: formData,
            });
        
        const { id: gameId } = (await response.json()) as { id: string };

        return gameId;
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
        downloadStatsbook,
        deleteGame,
        createGame,
        uploadGame,
        getCurrentGame,
        setCurrentGame,
        getGameState,
    }
}