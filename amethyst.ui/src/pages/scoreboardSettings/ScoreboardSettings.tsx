import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui"
import { useConfigurationApi, useI18n } from "@/hooks"
import { useConfiguration } from "@/hooks/ConfigurationHook";
import { DisplayConfiguration } from "@/types";
import { ConfigurationSwitch } from "./components/ConfigurationSwitch";

export const ScoreboardSettings = () => {

    const { translate } = useI18n();

    const configuration = useConfiguration<DisplayConfiguration>("DisplayConfiguration");

    const { setConfiguration } = useConfigurationApi();

    const handleShowSidebarsChanged = (checked: boolean) => {
        setConfiguration("DisplayConfiguration", { ...configuration, showSidebars: checked });
    }

    const handleUseTextBackgroundsChanged = (checked: boolean) => {
        setConfiguration("DisplayConfiguration", { ...configuration, useTextBackgrounds: checked });
    }

    return (
        <>
            <title>{translate("ScoreboardSettings.Title")} | {translate("Main.Title")}</title>
            <Card className="m-4">
                <CardHeader>
                    <CardTitle>Scoreboard display settings</CardTitle>
                </CardHeader>
                <CardContent className="flex flex-col gap-2">
                    <ConfigurationSwitch text="Show team color sidebars" checked={configuration?.showSidebars} onCheckedChanged={handleShowSidebarsChanged} />
                    <ConfigurationSwitch text="Use team colors for name" checked={configuration?.useTextBackgrounds} onCheckedChanged={handleUseTextBackgroundsChanged} />
                </CardContent>
            </Card>
        </>
    )
}