import { createContext, PropsWithChildren } from "react";
import { useHubConnection } from "./SignalRHubConnection";
import { ClientActivity } from "@/types";

type ClientConnectionContextProps = {

}

const ClientConnectionContext = createContext<ClientConnectionContextProps>({

});

export const useActivity = (activity: ClientActivity, path: string, gameId: string | null) => {
    
}

export const ClientConnectionContextProvider = ({ children }: PropsWithChildren) => {
    const { connection } = useHubConnection("clients");

    const handleChangeActivity = (activity: ClientActivity, gameId: string | null) => {

    }

    connection?.on("ChangeActivity", handleChangeActivity);

    return (
        <ClientConnectionContext.Provider value={{}}>
            { children }
        </ClientConnectionContext.Provider>
    )
}