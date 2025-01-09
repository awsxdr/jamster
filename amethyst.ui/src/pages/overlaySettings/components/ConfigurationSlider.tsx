import { Slider } from "@/components/ui";
import { cn } from "@/lib/utils";

type ConfigurationSwitchProps = {
    text?: string;
    value: number;
    min: number;
    max: number;
    step: number;
    onValueChanged?: (value: number) => void;
    onValueCommit?: (value: number) => void;
    className?: string;
}

export const ConfigurationSlider = ({ text, value, className, onValueChanged, onValueCommit, ...props }: ConfigurationSwitchProps) => {
    return (
        <div className={cn("flex gap-2 rounded-lg shadow-sm p-2 border", className)}>
            <div className="text-nowrap">{text}</div>
            <Slider value={[value]} {...props} onValueChange={v => onValueChanged?.(v[0])} onValueCommit={v => onValueCommit?.(v[0])} />
        </div>
    )
}