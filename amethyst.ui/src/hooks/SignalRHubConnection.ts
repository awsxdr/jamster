import * as SignalR from '@microsoft/signalr';
import { useEffect, useMemo, useState } from "react";
import { API_URL } from "@/constants";

export const useHubConnection = (hubPath?: string, onDisconnect?: () => Promise<void>) => {

    const [connection, setConnection] = useState<SignalR.HubConnection>();
    const [isConnected, setIsConnected] = useState(false);
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

        if(!hubConnection) {
            return () => {};
        }

        if(hubConnection.state !== "Disconnected") {
            return () => {};
        }

        (async () => {
            await hubConnection.start();

            console.debug("Hub connected", hubUrl);

            setConnection(hubConnection);
            setIsConnected(true);

            hubConnection.onclose(() => setIsConnected(false));
            hubConnection.onreconnecting(() => setIsConnected(false));
            hubConnection.onreconnected(() => setIsConnected(true));
        })();

    }, [hubConnection]);

    useEffect(() => {

        if(!connection) {
            return;
        }

        return () => {
            (async () => {
                await onDisconnect?.();
            })();
        }

    }, [connection]);

    return { connection, isConnected };
}
