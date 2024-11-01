import * as SignalR from '@microsoft/signalr';
import { useEffect, useMemo, useState } from "react";

export const API_URL = 'http://localhost:5249';

export const useHubConnection = (hubPath: string) => {

    const [connection, setConnection] = useState<SignalR.HubConnection>();

    console.log(`Connecting to hub at ${API_URL}/api/hubs/${hubPath}`);

    const hubConnection = useMemo(() => new SignalR.HubConnectionBuilder()
        .withUrl(`${API_URL}/api/hubs/${hubPath}`, { withCredentials: false })
        .withAutomaticReconnect({ nextRetryDelayInMilliseconds: context => {
            if(context.previousRetryCount < 10) {
                return 250;
            } else if(context.previousRetryCount < 40) {
                return 1000;
            } else {
                return 5000;
            }
        }})
        .build(), [hubPath]);

    useEffect(() => {
        (async () => {
            await hubConnection.start();
            console.log("Hub connected");
            setConnection(hubConnection);
        })();

        return () => {
            hubConnection.stop();
        }
    }, [hubConnection, setConnection]);

    return connection;
}
