import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { SidebarMenuButton, SidebarMenuItem } from "@/components/ui/sidebar";
import { useI18n } from "@/hooks/I18nHook";
import { useTheme } from "@/hooks/ThemeHook";
import { Check, ChevronUp, Palette } from "lucide-react";

export const ThemeMenu = () => {
    const { theme, setTheme } = useTheme();
    const { translate } = useI18n();

    return (
        <SidebarMenuItem>
            <DropdownMenu>
                <DropdownMenuTrigger asChild>
                    <SidebarMenuButton>
                        <Palette /> {translate("Theme")}
                        <ChevronUp className="ml-auto" />
                    </SidebarMenuButton>
                </DropdownMenuTrigger>
                <DropdownMenuContent
                    side="top"
                    className="w-[--radix-popper-anchor-width]"
                >
                    <DropdownMenuItem onClick={() => setTheme("light")}><span>{translate("Light")}</span> { theme === "light" ? <Check /> : <></> }</DropdownMenuItem>
                    <DropdownMenuItem onClick={() => setTheme("dark")}><span>{translate("Dark")}</span> { theme === "dark" ? <Check /> : <></> }</DropdownMenuItem>
                    <DropdownMenuItem onClick={() => setTheme("system")}><span>{translate("System default")}</span> { theme === "system" ? <Check /> : <></> }</DropdownMenuItem>
                </DropdownMenuContent>
            </DropdownMenu>
        </SidebarMenuItem>
    );
}