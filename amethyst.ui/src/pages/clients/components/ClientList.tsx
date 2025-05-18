import { useClients } from "@/hooks";
import { ClientDetails } from "./ClientDetails";
import { ClientActivity } from "@/types";
import { useMemo } from "react";

type ClientListProps = {
    filter?: ClientActivity[];
    blacklist?: boolean;
    changable?: boolean;
}

export const ClientList = ({ filter, blacklist, changable }: ClientListProps) => {

    const clients = useClients();

    const controllableClients = useMemo(() =>
        clients.filter(c => !filter || (blacklist ? !filter.includes(c.activityInfo.activity) : filter.includes(c.activityInfo.activity))),
        [clients, filter, blacklist]
    );

    return (
        <div className="flex flex-col gap-4 lg:grid lg:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4">
            {controllableClients.map(c => (<ClientDetails key={c.name} client={c} changable={changable} />))}
        </div>
    );
}