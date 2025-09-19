import { ColorSelectButton } from "@/components";
import { ConfigurationSlider } from "@/components/configuration";
import { Label, Switch } from "@/components/ui";
import { useI18n } from "@/hooks";
import { Color, StreamOverlayActivity } from "@/types";
import { useEffect, useMemo, useState } from "react";

type OverlaySettingsProps = {
    activity: StreamOverlayActivity;
    onActivityChanged: (scale: number, useBackground: boolean, backgroundColor: string) => void;
}

export const OverlaySettings = ({ activity, onActivityChanged }: OverlaySettingsProps) => {

    const { translate } = useI18n({ prefix: "Clients.OverlaySettings." });

    const [tempScale, setTempScale] = useState(activity.scale);

    const handleScaleChanged = (scale: number) => {
        onActivityChanged(scale, activity.useBackground, activity.backgroundColor);
    }

    const handleUseBackgroundChanged = (useBackground: boolean) => {
        onActivityChanged(activity.scale, useBackground, activity.backgroundColor);
    }
    
    const backgroundColor = useMemo(() => Color.rgbToHsl(Color.parseRgb(activity.backgroundColor) ?? { red: 0, green: 1, blue: 0 }), [activity]);

    const [tempBackgroundColor, setTempBackgroundColor] = useState(backgroundColor);

    useEffect(() => setTempBackgroundColor(backgroundColor), [backgroundColor]);

    const handleBackgroundColorClosed = () => {
        onActivityChanged(activity.scale, activity.useBackground, Color.rgbToString(Color.hslToRgb(tempBackgroundColor)));
    }
    
    return (
        <>
            <div className="flex flex-col gap-1">
                <Label>{translate("Scale")}</Label>
                <ConfigurationSlider 
                    text={`${Math.floor(tempScale * 100)}%`}
                    value={tempScale}
                    min={0.5}
                    max={3}
                    step={0.1}
                    onValueChanged={setTempScale}
                    onValueCommit={handleScaleChanged}
                />
            </div>
            <div className="flex flex-col gap-1">
                <Label className="flex gap-3 items-center">
                    {translate("Background")}
                    <Switch checked={activity.useBackground} onCheckedChange={handleUseBackgroundChanged} />
                </Label>
                <ColorSelectButton 
                    color={tempBackgroundColor} 
                    disabled={!activity.useBackground} 
                    onColorChanged={setTempBackgroundColor} 
                    onClose={handleBackgroundColorClosed}
                />
            </div>
        </>
    );
}
