import { ColorSelectButton } from "@/components";
import { ConfigurationLanguageSelect, ConfigurationSlider } from "@/components/configuration";
import { ConfigurationSetting } from "@/components/configuration/ConfigurationSetting";
import { Card, CardContent, CardHeader, CardTitle, Switch } from "@/components/ui"
import { useConfiguration, useI18n } from "@/hooks"
import { Color, HslColor, OverlayConfiguration } from "@/types";
import { useEffect, useMemo, useState } from "react";

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

    const backgroundColor = useMemo(() => Color.rgbToHsl(Color.parseRgb(configuration?.backgroundColor ?? "#00ff00") ?? { red: 0, green: 1, blue: 0 }), [configuration]);

    if(!configuration) {
        return (
            <></>
        );
    }

    const handleLanguageChanged = (newLanguage: string) => {
        setConfiguration({ ...configuration, language: newLanguage});
    }

    const handleScaleChanged = (value: number) => {
        setConfiguration({ ...configuration, scale: value });
    }

    const handleUseBackgroundChanged = (checked: boolean) => {
        setConfiguration({ ...configuration, useBackground: checked });
    }

    const handleBackgroundColorChanged = (color: HslColor) => {
        setConfiguration({ ...configuration, backgroundColor: Color.rgbToString(Color.hslToRgb(color))});
    }

    return (
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
                <ConfigurationSetting
                    text=""
                >
                    <Switch checked={configuration.useBackground} onCheckedChange={handleUseBackgroundChanged} />
                    {translate("OverlaySettings.FillBackground")}
                    <ColorSelectButton 
                        color={backgroundColor} 
                        disabled={!configuration.useBackground} 
                        onColorChanged={handleBackgroundColorChanged} 
                    />
                </ConfigurationSetting>
            </CardContent>
        </Card>
    )
}