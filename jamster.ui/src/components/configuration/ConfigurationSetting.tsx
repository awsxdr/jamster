import { cn } from "@/lib/utils";
import { PropsWithChildren } from "react";

export type ConfigurationSettingProps = {
    text?: string;
    textRight?: boolean;
    className?: string;
}

export const ConfigurationSetting = ({ text, textRight, className, children }: PropsWithChildren<ConfigurationSettingProps>) => (
    <div className={cn("flex items-center gap-2 rounded-lg shadow-sm p-2 border", className)}>
        { textRight && children }
        <div className="text-nowrap">{text}</div>
        { !textRight && children }
    </div>
);
