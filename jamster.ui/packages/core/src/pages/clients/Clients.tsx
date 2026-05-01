import { useI18n } from "@/hooks";
import { ClientList } from "./components";
import { ClientActivity } from "@/types";

const CONTROLLABLE_ACTIVITIES = [ClientActivity.Scoreboard, ClientActivity.StreamOverlay, ClientActivity.PenaltyWhiteboard];

export const Clients = () => {

    const { translate } = useI18n({ prefix: "Clients." });

    return (
        <>
            <title>{translate("Title")} | {translate("Main.Title", { ignorePrefix: true })}</title>
            <h1 className="p-4 text-xl">{translate("Title")}</h1>
            <div className="p-4 pt-0">
                <ClientList filter={CONTROLLABLE_ACTIVITIES} changable />
            </div>
            {/* <div className="p-4">
                <ClientList filter={CONTROLLABLE_ACTIVITIES} blacklist />
            </div> */}
        </>
    );
}