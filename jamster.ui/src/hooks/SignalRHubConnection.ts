import * as SignalR from '@microsoft/signalr';
import { useEffect, useMemo, useState } from "react";
import { API_URL } from "@/constants";

export const useHubConnection = (hubPath?: string, onDisconnect?: () => Promise<void>) => {

    const [connection, setConnection] = useState<SignalR.HubConnection>();
    const [isConnected, setIsConnected] = useState(false);
    const hubUrl = useMemo(() => `${API_URL}/api/Hubs/${hubPath}`, [hubPath]);

    useEffect(() => {

        if (!hubPath) {
            return;
        }

        let stopped = false;

        const hubConnection = new SignalR.HubConnectionBuilder()
            .withUrl(hubUrl, { withCredentials: false })
            .withAutomaticReconnect({ nextRetryDelayInMilliseconds: context => {
                if (context.previousRetryCount < 10) {
                    return 250;
                } else if (context.previousRetryCount < 40) {
                    return 1000;
                } else {
                    return 5000;
                }
            }})
            .build();

        (async () => {
            try {
                await hubConnection.start();
            } catch {
                return;
            }

            if (stopped) return;

            console.debug("Hub connected", hubUrl);

            setConnection(hubConnection);
            setIsConnected(true);

            hubConnection.onclose(() => setIsConnected(false));
            hubConnection.onreconnecting(() => setIsConnected(false));
            hubConnection.onreconnected(() => setIsConnected(true));
        })();

        return () => {
            stopped = true;
            hubConnection.stop();
        };

    }, [hubUrl]);

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
