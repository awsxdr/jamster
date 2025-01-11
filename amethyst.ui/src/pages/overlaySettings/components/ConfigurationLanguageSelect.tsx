import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui";
import { LanguageName } from "@/hooks";
import { cn } from "@/lib/utils";

type ConfigurationLanguageSelectProps = {
    text: string;
    language: string;
    languages: LanguageName[];
    className?: string;
    onSelectedChanged?: (selected: string) => void;
}

export const ConfigurationLanguageSelect = ({ text, language, languages, className, onSelectedChanged }: ConfigurationLanguageSelectProps) => {
    return (
        <>
            <div className={cn("flex gap-2 rounded-lg shadow-sm p-2 border items-baseline w-full", className)}>
                <div className="text-nowrap">{text}</div>
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
            </div>
        </>
    );
}