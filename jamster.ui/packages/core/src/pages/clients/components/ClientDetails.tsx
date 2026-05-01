import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui";
import { Client, ClientActivity } from "@/types";
import { useClientsApi } from "@/hooks";
import { ActivitySelect, GameSelect, LanguageSelect } from ".";
import { OverlaySettings } from "./OverlaySettings";
import { ScoreboardSettings } from "./ScoreboardSettings";

type ClientDetailsProps = {
    client: Client;
    changable?: boolean;
}

export const ClientDetails = ({ client, changable }: ClientDetailsProps) => {

    const { setConnectionActivity } = useClientsApi();

    const handleActivityChanged = (activity: ClientActivity) => {
        switch(activity) {

            case ClientActivity.Scoreboard:
                setConnectionActivity(client.name, { ...client.activityInfo, activity, useSidebars: true, useNameBackgrounds: true });
                break;

            case ClientActivity.StreamOverlay:
                setConnectionActivity(client.name, { ...client.activityInfo, activity, scale: 1.0, useBackground: false, backgroundColor: '#00ff00' });
                break;

            default:
                setConnectionActivity(client.name, { ...client.activityInfo, activity });
                break;
        }
    }

    const handleGameIdChanged = (gameId: string) => {
        setConnectionActivity(client.name, { ...client.activityInfo, gameId });
    }

    const handleLanguageChanged = (languageCode: string) => {
        setConnectionActivity(client.name, { ...client.activityInfo, languageCode });
    }

    const handleScoreboardActivityChanged = (useSidebars: boolean, useNameBackgrounds: boolean) => {
        setConnectionActivity(client.name, { ...client.activityInfo, activity: ClientActivity.Scoreboard, useSidebars, useNameBackgrounds });
    }

    const handleStreamOverlayActivityChanged = (scale: number, useBackground: boolean, backgroundColor: string) => {
        setConnectionActivity(client.name, { ...client.activityInfo, activity: ClientActivity.StreamOverlay, scale, useBackground, backgroundColor });
    }

    return (
        <Card>
            <CardHeader>
                <CardTitle>{client.name}</CardTitle>
                <div>
                    <div className="text-xs">{client.ipAddress}</div>
                </div>
            </CardHeader>
            <CardContent className="flex flex-col gap-2">
                { changable && (
                    <>
                        <ActivitySelect activity={client.activityInfo.activity} onActivityChanged={handleActivityChanged} />
                        <GameSelect selectedGameId={client.activityInfo.gameId || "current"} onSelectedGameIdChanged={handleGameIdChanged} />
                        <LanguageSelect language={client.activityInfo.languageCode} onLanguageChanged={handleLanguageChanged} />
                        {client.activityInfo.activity === ClientActivity.Scoreboard && (
                            <ScoreboardSettings activity={client.activityInfo} onActivityChanged={handleScoreboardActivityChanged} />
                        )}
                        {client.activityInfo.activity === ClientActivity.StreamOverlay && (
                            <OverlaySettings activity={client.activityInfo} onActivityChanged={handleStreamOverlayActivityChanged} />
                        )}
                    </>
                )}
            </CardContent>
        </Card>
    );
}