import { useClients } from "@/hooks";
import { ClientDetails } from "./ClientDetails";
import { ClientActivity } from "@/types";
import { useMemo } from "react";

const CONTROLLABLE_ACTIVITIES = [ClientActivity.Scoreboard, ClientActivity.StreamOverlay, ClientActivity.PenaltyWhiteboard];

export const ClientList = () => {

    const clients = useClients();

    console.log(clients);

    const controllableClients = useMemo(() =>
        clients.filter(c => CONTROLLABLE_ACTIVITIES.includes(c.currentActivity)),
        [clients]
    );

    return (
        <div className="flex flex-col gap-4 lg:grid grid-flow-col auto-cols-fr">
            {controllableClients.map(c => (<ClientDetails key={c.id} client={c} />))}
        </div>
    );
}