import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui"
import { useConfiguration, useI18n } from "@/hooks"
import { DisplayConfiguration } from "@/types";
import { ConfigurationLanguageSelect, ConfigurationSwitch } from "@/components/configuration";

export const ScoreboardSettings = () => {

    const { translate, languages } = useI18n();

    const { configuration, setConfiguration } = useConfiguration<DisplayConfiguration>("DisplayConfiguration");

    if(!configuration) {
        return (<></>);
    }

    const handleLanguageChanged = (newLanguage: string) => {
        setConfiguration({ ...configuration, language: newLanguage});
    }

    const handleShowSidebarsChanged = (checked: boolean) => {
        setConfiguration({ ...configuration, showSidebars: checked });
    }

    const handleUseTextBackgroundsChanged = (checked: boolean) => {
        setConfiguration({ ...configuration, useTextBackgrounds: checked });
    }

    return (
        <Card className="m-4">
            <CardHeader>
                <CardTitle>{translate("ScoreboardSettings.Title")}</CardTitle>
            </CardHeader>
            <CardContent className="flex flex-col gap-2">
                <ConfigurationLanguageSelect 
                    text={translate("ScoreboardSettings.Language")} 
                    language={configuration.language} 
                    languages={languages} 
                    onSelectedChanged={handleLanguageChanged}
                />
                <ConfigurationSwitch text={translate("ScoreboardSettings.ShowSidebars")} checked={configuration?.showSidebars} onCheckedChanged={handleShowSidebarsChanged} />
                <ConfigurationSwitch text={translate("ScoreboardSettings.UseTextBackgrounds")} checked={configuration?.useTextBackgrounds} onCheckedChanged={handleUseTextBackgroundsChanged} />
            </CardContent>
        </Card>
    )
}