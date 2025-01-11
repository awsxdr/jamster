import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui"
import { useConfiguration, useI18n } from "@/hooks"
import { OverlayConfiguration } from "@/types";
import { ConfigurationSlider } from "./components/ConfigurationSlider";
import { useEffect, useState } from "react";
import { ConfigurationLanguageSelect } from "./components/ConfigurationLanguageSelect";
import { MobileSidebarTrigger } from "@/components/MobileSidebarTrigger";

export const OverlaySettings = () => {

    const { translate, languages } = useI18n();

    const { configuration, setConfiguration } = useConfiguration<OverlayConfiguration>("OverlayConfiguration");

    const [tempScale, setTempScale] = useState(0);

    useEffect(() => {
        if(!configuration) {
            return;
        }

        setTempScale(configuration.scale);
    }, [configuration?.scale]);

    if(!configuration) {
        return (
            <></>
        );
    }

    const handleLanguageChanged = (newLanguage: string) => {
        console.log("Set language", newLanguage);
        setConfiguration({ ...configuration, language: newLanguage});
    }

    const handleScaleChanged = (value: number) => {
        console.log("Set scale", value);
        setConfiguration({ ...configuration, scale: value });
    }

    return (
        <>
            <title>{translate("OverlaySettings.Title")} | {translate("Main.Title")}</title>
            <MobileSidebarTrigger className="mx-5 mt-3" />
            <Card className="m-4">
                <CardHeader>
                    <CardTitle>{translate("OverlaySettings.Title")}</CardTitle>
                </CardHeader>
                <CardContent className="flex flex-col gap-2">
                    <ConfigurationLanguageSelect 
                        text={translate("OverlaySettings.Language")} 
                        language={configuration.language} 
                        languages={languages} 
                        onSelectedChanged={handleLanguageChanged}
                    />
                    <ConfigurationSlider 
                        text={translate("OverlaySettings.Scale")}
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