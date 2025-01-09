import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui"
import { useConfigurationApi, useI18n } from "@/hooks"
import { useConfiguration } from "@/hooks/ConfigurationHook";
import { OverlayConfiguration } from "@/types";
import { ConfigurationSlider } from "./components/ConfigurationSlider";
import { useEffect, useState } from "react";

export const OverlaySettings = () => {

    const { translate } = useI18n();

    const { scale } = useConfiguration<OverlayConfiguration>("OverlayConfiguration") ?? { scale: 1.0 };

    const { setConfiguration } = useConfigurationApi();
    const [tempScale, setTempScale] = useState(0);

    useEffect(() => {
        setTempScale(scale);
    }, [scale])

    const handleScaleChanged = (value: number) => {
        setConfiguration("OverlayConfiguration", { scale: value });
    }

    return (
        <>
            <title>{translate("OverlaySettings.Title")} | {translate("Main.Title")}</title>
            <Card className="m-4">
                <CardHeader>
                    <CardTitle>Scoreboard display settings</CardTitle>
                </CardHeader>
                <CardContent className="flex flex-col gap-2">
                    <ConfigurationSlider 
                        text="Overlay scale"
                        value={tempScale}
                        min={0.5}
                        max={3}
                        step={0.1}
                        onValueChanged={setTempScale}
                        onValueCommit={handleScaleChanged}
                    />
                </CardContent>
            </Card>
        </>
    )
}