import { MobileSidebarTrigger } from "@/components";
import { useI18n } from "@/hooks";
import { OverlaySettings } from "./components/OverlaySettings";
import { ScoreboardSettings } from "./components/ScoreboardSettings";

export const Settings = () => {
    const { translate } = useI18n({ prefix: "Settings." });

    return (
        <>
            <title>{translate("Title")} | {translate("Main.Title", { ignorePrefix: true })}</title>
            <MobileSidebarTrigger className="mx-5 mt-3" />
            <ScoreboardSettings />
            <OverlaySettings />
        </>
    );
}