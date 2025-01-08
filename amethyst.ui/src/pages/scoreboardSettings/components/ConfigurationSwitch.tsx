import { Switch } from "@/components/ui";
import { cn } from "@/lib/utils";

type ConfigurationSwitchProps = {
    text?: string;
    checked?: boolean;
    onCheckedChanged?: (checked: boolean) => void;
    className?: string;
}

export const ConfigurationSwitch = ({ text, checked, onCheckedChanged, className }: ConfigurationSwitchProps) => {
    return (
        <div className={cn("flex gap-2 rounded-lg shadow-sm p-2 border", className)}>
            <Switch checked={checked} onCheckedChange={onCheckedChanged} />
            <div>{text}</div>
        </div>
    )
}