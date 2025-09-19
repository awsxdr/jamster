import { Slider } from "@/components/ui";
import { ConfigurationSetting, ConfigurationSettingProps } from "./ConfigurationSetting";

type ConfigurationSwitchProps = ConfigurationSettingProps & {
    value: number;
    min: number;
    max: number;
    step: number;
    onValueChanged?: (value: number) => void;
    onValueCommit?: (value: number) => void;
}

export const ConfigurationSlider = ({ text, value, className, onValueChanged, onValueCommit, ...props }: ConfigurationSwitchProps) => (
    <ConfigurationSetting text={text} className={className}>
        <Slider value={[value]} {...props} onValueChange={v => onValueChanged?.(v[0])} onValueCommit={v => onValueCommit?.(v[0])} />
    </ConfigurationSetting>
);
