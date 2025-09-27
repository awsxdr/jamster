import { useHasServerConnection, useI18n } from "@/hooks"
import { Alert, AlertDescription, AlertTitle } from "./ui";
import { WifiOff } from "lucide-react";

export const ConnectionLostAlert = () => {
    const { translate } = useI18n({ prefix: "ConnectionLostAlert." });
    const hasConnection = useHasServerConnection();

    return !hasConnection && (
        <Alert id="ConnectionLostAlert" className="rounded-none" variant="destructive">
            <WifiOff />
            <AlertTitle className="ml-2">{translate("Title")}</AlertTitle>
            <AlertDescription className="ml-2">{translate("Description")}</AlertDescription>
        </Alert>
    );
}