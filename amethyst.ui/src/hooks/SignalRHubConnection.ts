import * as SignalR from '@microsoft/signalr';
import { useEffect, useMemo, useState } from "react";

export const API_URL = 'http://localhost:5249';

export const useHubConnection = (hubPath: string) => {

    const [connection, setConnection] = useState<SignalR.HubConnection>();
    const hubUrl = useMemo(() => `${API_URL}/api/Hubs/${hubPath}`, [hubPath]);

    const hubConnection = useMemo(() => {
        console.log("Connecting to hub", hubUrl);

        return new SignalR.HubConnectionBuilder()
            .withUrl(hubUrl, { withCredentials: false })
            .withAutomaticReconnect({ nextRetryDelayInMilliseconds: context => {
                if(context.previousRetryCount < 10) {
                    return 250;
                } else if(context.previousRetryCount < 40) {
                    return 1000;
                } else {
                    return 5000;
                }
            }})
            .build();
    }, [hubUrl]);

    useEffect(() => {
        (async () => {
            console.log("Starting connection", hubUrl);
            try {
                await hubConnection.start();
                console.log("Hub connected", hubUrl);
                setConnection(hubConnection);
            } catch(error) {
                console.error("Error while starting hub connection", hubUrl, error);
            }
        })();

        // return () => {
        //     console.log("Stopping connection", hubUrl);
        //     hubConnection.stop();
        //     setReconnect(r => r += 1);
        // }
    }, [hubConnection, setConnection]);

    return connection;
}
