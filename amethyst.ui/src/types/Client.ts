import { ActivityData } from "./ActivityData";

export type Client = {
    connectionId: string;
    name: string;
    ipAddress: string;
    activityInfo: ActivityData;
    lastUpdateTime: string;
}