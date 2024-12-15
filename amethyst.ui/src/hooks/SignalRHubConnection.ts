import * as SignalR from '@microsoft/signalr';
import { useEffect, useMemo, useState } from "react";
import { API_URL } from "@/constants";

export const useHubConnection = (hubPath?: string) => {

    const [connection, setConnection] = useState<SignalR.HubConnection>();
    const hubUrl = useMemo(() => `${API_URL}/api/Hubs/${hubPath}`, [hubPath]);

    const hubConnection = useMemo(() => {
        console.debug("Connecting to hub", hubUrl);

        if(!hubPath) {
            return;
        }

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
            console.debug("Starting connection", hubUrl);
            try {
                if(hubConnection?.state !== "Disconnected") {
                    return;
                }

                await hubConnection.start();
                console.debug("Hub connected", hubUrl);
                setConnection(hubConnection);
            } catch(error) {
                console.error("Error while starting hub connection", hubUrl, error);
            }
        })();
    }, [hubConnection, setConnection]);

    return connection;
}
