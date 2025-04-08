import { ClientActivity } from ".";

export type Client = {
    id: string;
    name: { name: string, isCustom: boolean };
    currentActivity: ClientActivity;
    path: string;
    gameId: string | null;
    lastUpdateTime: string;
}