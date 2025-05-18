import { Label, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui"
import { useI18n } from "@/hooks";
import { ClientActivity } from "@/types"

type LanguageSelectProps = {
    language: string;
    onLanguageChanged: (activity: ClientActivity) => void;
}

export const LanguageSelect = ({ language, onLanguageChanged }: LanguageSelectProps) => {

    const { translate, languages } = useI18n({ prefix: "Clients.LanguageSelect." })

    return (
        <div className="flex flex-col gap-1">
            <Label>{translate("Label")}</Label>
            <Select value={language} onValueChange={onLanguageChanged}>
                <SelectTrigger>
                    <SelectValue />
                </SelectTrigger>
                <SelectContent>
                    { languages.map(l => <SelectItem key={l.code} value={l.code}>{l.displayName}</SelectItem>)}
                </SelectContent>
            </Select>
        </div>
    )
}