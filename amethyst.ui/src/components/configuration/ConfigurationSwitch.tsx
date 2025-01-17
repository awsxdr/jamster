import { Switch } from "@/components/ui";
import { ConfigurationSetting, ConfigurationSettingProps } from "./ConfigurationSetting";

type ConfigurationSwitchProps = ConfigurationSettingProps & {
    checked?: boolean;
    onCheckedChanged?: (checked: boolean) => void;
}

export const ConfigurationSwitch = ({ text, checked, onCheckedChanged, className }: ConfigurationSwitchProps) => {
    return (
        <ConfigurationSetting text={text} className={className} textRight>
            <Switch checked={checked} onCheckedChange={onCheckedChanged} />
        </ConfigurationSetting>
    )
}