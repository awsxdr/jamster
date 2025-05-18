import { API_URL } from "@/constants";
import { ActivityData, Client } from "@/types"

type ClientsApi = {
    getConnectedClients: () => Promise<Client[]>;
    getClient: (clientName: string) => Promise<Client>;
    setConnectionName: (clientName: string, newName: string) => Promise<void>;
    setConnectionActivity: (clientName: string, activity: ActivityData) => Promise<void>;
}

export const useClientsApi: () => ClientsApi = () => {

    const getConnectedClients = async () => {
        const response = await fetch(`${API_URL}/api/clients`);
        return (await response.json()) as Client[];
    }

    const getClient = async (clientName: string) => {
        const response = await fetch(`${API_URL}/api/clients/${clientName}`);
        return (await response.json()) as Client;
    }

    const setConnectionName = async (clientName: string, newName: string) => {
        await fetch(`${API_URL}/api/clients/${clientName}/name`, {
            method: 'PUT',
            body: JSON.stringify({ name: newName }),
            headers: {
                "Content-Type": "application/json"
            },
        });
    }

    const setConnectionActivity = async (clientName: string, activity: ActivityData) => {
        await fetch(`${API_URL}/api/clients/${clientName}/activity`, {
            method: 'PUT',
            body: JSON.stringify({ activityDetails: activity }),
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