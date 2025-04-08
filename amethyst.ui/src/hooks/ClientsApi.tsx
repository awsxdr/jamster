import { API_URL } from "@/constants";
import { Client, ClientActivity } from "@/types"

type ClientsApi = {
    getConnectedClients: () => Promise<Client[]>;
    getClient: (connectionId: string) => Promise<Client>;
    setConnectionName: (connectionId: string, name: string) => Promise<void>;
    setConnectionActivity: (connectionId: string, activity: ClientActivity, gameId: string | null) => Promise<void>;
}

export const useClientsApi: () => ClientsApi = () => {

    const getConnectedClients = async () => {
        const response = await fetch(`${API_URL}/api/clients`);
        return (await response.json()) as Client[];
    }

    const getClient = async (connectionId: string) => {
        const response = await fetch(`${API_URL}/api/clients/${connectionId}`);
        return (await response.json()) as Client;
    }

    const setConnectionName = async (connectionId: string, name: string) => {
        await fetch(`${API_URL}/api/clients/${connectionId}/name`, {
            method: 'PUT',
            body: JSON.stringify({ name }),
            headers: {
                "Content-Type": "application/json"
            },
        });
    }

    const setConnectionActivity = async (connectionId: string, activity: ClientActivity, gameId: string | null) => {
        await fetch(`${API_URL}/api/clients/${connectionId}/activity`, {
            method: 'PUT',
            body: JSON.stringify({ activity, gameId }),
            headers: {
                "Content-Type": "application/json"
            },
        });
    }

    return {
        getConnectedClients,
        getClient,
        setConnectionName,
        setConnectionActivity,
    }
}