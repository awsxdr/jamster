import { Label, Switch } from "@/components/ui";
import { useI18n } from "@/hooks";
import { ScoreboardActivity } from "@/types";

type ScoreboardSettingsProps = {
    activity: ScoreboardActivity;
    onActivityChanged: (useSidebars: boolean, useNameBackgrounds: boolean) => void;
}

export const ScoreboardSettings = ({ activity, onActivityChanged }: ScoreboardSettingsProps) => {

    const { translate } = useI18n({ prefix: "Clients.ScoreboardSettings." });

    const handleUseSidebarsChanged = (useSidebars: boolean) => {
        onActivityChanged(useSidebars, activity.useNameBackgrounds);
    }

    const handleUseNameBackgroundsChanged = (useNameBackgrounds: boolean) => {
        onActivityChanged(activity.useSidebars, useNameBackgrounds);
    }
    
    return (
        <>
            <div className="flex flex-col gap-1">
                <Label className="flex gap-3 items-center">
                    {translate("Sidebars")}
                    <Switch checked={activity.useSidebars} onCheckedChange={handleUseSidebarsChanged} />
                </Label>
            </div>
            <div className="flex flex-col gap-1">
                <Label className="flex gap-3 items-center">
                    {translate("NameBackgrounds")}
                    <Switch checked={activity.useNameBackgrounds} onCheckedChange={handleUseNameBackgroundsChanged} />
                </Label>
            </div>
        </>
    );
}
