import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { SidebarMenuButton, SidebarMenuItem } from "@/components/ui/sidebar";
import { LanguageName, useI18n } from "@/hooks/I18nHook";
import { Check, ChevronUp, Globe } from "lucide-react";

type LanguageItemProps = LanguageName;

const LanguageItem = ({ code, displayName }: LanguageItemProps) => {
    const { language: currentLanguage, setLanguage } = useI18n();

    const changeLanguage = () => {
        setLanguage(code);
    }

    return (
        <DropdownMenuItem onClick={changeLanguage}><span>{displayName}</span> { currentLanguage === code ? <Check /> : <></> }</DropdownMenuItem>
    )
}

export const LanguageMenu = () => {
    const { translate, languages } = useI18n();

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
                    { languages.map(l => <LanguageItem key={l.code} {...l} />)}
                </DropdownMenuContent>
            </DropdownMenu>
        </SidebarMenuItem>
    );
}