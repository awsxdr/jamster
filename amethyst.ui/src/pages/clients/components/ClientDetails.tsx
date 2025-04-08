import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui";
import { Client, ClientActivity } from "@/types";
import { ActivitySelect } from "./ActivitySelect";
import { useClientsApi } from "@/hooks";
import { GameSelect } from "./GameSelect";

type ClientDetailsProps = {
    client: Client;
}

export const ClientDetails = ({ client }: ClientDetailsProps) => {

    const { setConnectionActivity } = useClientsApi();

    const handleActivityChanged = (activity: ClientActivity) => {
        setConnectionActivity(client.id, activity, client.gameId);
    }

    const handleGameIdChanged = (gameId: string) => {
        setConnectionActivity(client.id, client.currentActivity, gameId);
    }

    return (
        <Card>
            <CardHeader>
                <CardTitle>{client.name.name}</CardTitle>
            </CardHeader>
            <CardContent className="flex flex-col gap-2">
                <ActivitySelect activity={client.currentActivity} onActivityChanged={handleActivityChanged} />
                <GameSelect selectedGameId={client.gameId || "current"} onSelectedGameIdChanged={handleGameIdChanged} />
            </CardContent>
        </Card>
    );
}