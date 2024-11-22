import { Button } from "@/components/ui/button";
import { Sidebar, SidebarContent, SidebarFooter, SidebarGroup, SidebarGroupContent, SidebarGroupLabel, SidebarHeader, SidebarMenu, SidebarMenuButton, SidebarMenuItem, useSidebar } from "@components/ui/sidebar"
import { ChevronLeft, ChevronRight, Keyboard, List, TvMinimal, Users } from "lucide-react"
import { ReactNode } from "react";
import { Link, useLocation } from "react-router-dom";
import { LanguageMenu } from "./LanguageMenu";
import { ThemeMenu } from "./ThemeMenu";
import { useI18n } from "@/hooks/I18nHook";

type SidebarItem = {
    title: string;
    href: string;
    icon?: ReactNode;
};

type SidebarItemList = {
    [group: string]: SidebarItem[];
};

const sidebarItems: SidebarItemList = {
    "Main": [
        {
            title: "MenuSidebar.ScoreboardDisplay",
            href: "/scoreboard",
            icon: <TvMinimal />
        },
        {
            title: "MenuSidebar.ScoreboardControl",
            href: "/control",
            icon: <Keyboard />
        }
    ],
    "Data": [
        {
            title: "MenuSidebar.Teams",
            href: "/teams",
            icon: <Users />
        },
        {
            title: "MenuSidebar.Games",
            href: "/games",
            icon: <List />
        }
    ],
}

export const MenuSidebar = () => {
    const location = useLocation();

    const { open: sidebarOpen, toggleSidebar } = useSidebar();
    const { translate } = useI18n();

    return (
        <Sidebar collapsible="icon">
            <SidebarHeader className="w-full items-end">
                <Button onClick={toggleSidebar} variant="ghost" size="sm">
                    { sidebarOpen ? <ChevronLeft /> : <ChevronRight /> }
                </Button>
            </SidebarHeader>
            <SidebarContent>
                {
                    Object.keys(sidebarItems).map(groupName => {
                        const items = sidebarItems[groupName];

                        return (
                            <SidebarGroup key={groupName}>
                                <SidebarGroupLabel>{translate(groupName)}</SidebarGroupLabel>
                                <SidebarGroupContent>
                                    <SidebarMenu>
                                        {items.map(item =>
                                            <SidebarMenuItem key={item.title}>
                                                <SidebarMenuButton asChild isActive={location.pathname.startsWith(item.href)}>
                                                    <Link to={item.href}>
                                                        {item.icon}
                                                        <span>{translate(item.title)}</span>
                                                    </Link>
                                                </SidebarMenuButton>
                                            </SidebarMenuItem>
                                        )}
                                    </SidebarMenu>
                                </SidebarGroupContent>
                            </SidebarGroup>
                        );
                    })
                }
           </SidebarContent>
           <SidebarFooter>
                <SidebarMenu>
                    <LanguageMenu />
                    <ThemeMenu />
                </SidebarMenu>
            </SidebarFooter>
        </Sidebar>
    )
}