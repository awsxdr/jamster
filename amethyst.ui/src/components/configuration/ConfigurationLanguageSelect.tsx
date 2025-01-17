import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui";
import { LanguageName } from "@/hooks";
import { ConfigurationSetting, ConfigurationSettingProps } from "./ConfigurationSetting";

type ConfigurationLanguageSelectProps = ConfigurationSettingProps & {
    language: string;
    languages: LanguageName[];
    onSelectedChanged?: (selected: string) => void;
}

export const ConfigurationLanguageSelect = ({ text, language, languages, className, onSelectedChanged }: ConfigurationLanguageSelectProps) => (
    <ConfigurationSetting text={text} className={className}>
        <Select value={language} onValueChange={onSelectedChanged}>
            <SelectTrigger className="w-full lg:w-[580px]">
                <SelectValue />
            </SelectTrigger>
            <SelectContent>
                {
                    languages.map(l => <SelectItem key={l.code} value={l.code}>{l.displayName}</SelectItem>)
                }
            </SelectContent>
        </Select>
    </ConfigurationSetting>
);
