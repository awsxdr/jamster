import { useI18n } from "@/hooks";
import { ClientList } from "./components";

export const Clients = () => {

    const { translate } = useI18n({ prefix: "Clients." })

    return (
        <>
            <title>{translate("Title")} | {translate("Main.Title", { ignorePrefix: true })}</title>
            <div className="p-4">
                <ClientList />
            </div>
        </>
    );
}