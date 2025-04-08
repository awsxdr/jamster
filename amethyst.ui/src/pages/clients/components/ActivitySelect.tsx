import { Label, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui"
import { useI18n } from "@/hooks";
import { ClientActivity } from "@/types"

type ActivitySelectProps = {
    activity: ClientActivity;
    onActivityChanged: (activity: ClientActivity) => void;
}

export const ActivitySelect = ({ activity, onActivityChanged }: ActivitySelectProps) => {

    const { translate } = useI18n({ prefix: "Clients.ActivitySelect." })

    return (
        <div className="flex flex-col gap-1">
            <Label>Screen</Label>
            <Select value={activity} onValueChange={onActivityChanged}>
                <SelectTrigger>
                    <SelectValue />
                </SelectTrigger>
                <SelectContent>
                    <SelectItem value={ClientActivity.Scoreboard}>{translate("Scoreboard")}</SelectItem>
                    <SelectItem value={ClientActivity.StreamOverlay}>{translate("Overlay")}</SelectItem>
                    <SelectItem value={ClientActivity.PenaltyWhiteboard}>{translate("Whiteboard")}</SelectItem>
                </SelectContent>
            </Select>
        </div>
    )
}