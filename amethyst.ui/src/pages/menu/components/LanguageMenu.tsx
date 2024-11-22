import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { SidebarMenuButton, SidebarMenuItem } from "@/components/ui/sidebar";
import { useI18n } from "@/hooks/I18nHook";
import { Check, ChevronUp, Globe } from "lucide-react";

type LanguageItemProps = {
    language: string;
    displayName: string;
}

const LanguageItem = ({ language, displayName }: LanguageItemProps) => {
    const { language: currentLanguage, setLanguage } = useI18n();

    const changeLanguage = () => {
        setLanguage(language);
    }

    return (
        <DropdownMenuItem onClick={changeLanguage}><span>{displayName}</span> { currentLanguage === language ? <Check /> : <></> }</DropdownMenuItem>
    )
}

export const LanguageMenu = () => {
    const { translate } = useI18n();

    const languages = [
        { language: "en", displayName: "English" },
        { language: "es", displayName: "Español" },
        { language: "dev", displayName: "Test" },
    ];

    return (
        <SidebarMenuItem>
            <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <SidebarMenuButton>
                        <Globe /> {translate("LanguageMenu.Title")}
                        <ChevronUp className="ml-auto" />
                    </SidebarMenuButton>
                </DropdownMenuTrigger>
                <DropdownMenuContent
                    side="top"
                    className="w-[--radix-popper-anchor-width]"
                >
                    { languages.map(l => <LanguageItem key={l.language} {...l} />)}
                </DropdownMenuContent>
            </DropdownMenu>
        </SidebarMenuItem>
    );
}