import { Button, Collapsible, CollapsibleContent, CollapsibleTrigger, Sidebar, SidebarContent, SidebarFooter, SidebarGroup, SidebarGroupContent, SidebarGroupLabel, SidebarHeader, SidebarMenu, SidebarMenuButton, SidebarMenuItem, useSidebar } from "@components/ui"
import { Captions, ChartNoAxesCombined, ChevronDown, ChevronLeft, ChevronRight, CircleHelp, Keyboard, List, MonitorCog, TvMinimal, Users } from "lucide-react"
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

type SidebarGroup = {
    collapsible?: boolean;
    defaultOpen?: boolean;
    items: SidebarItem[];
}

type SidebarGroupList = {
    [group: string]: SidebarGroup;
};

const sidebarItems: SidebarGroupList = {
    "MenuSidebar.MainGroup": {
        items: [
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
    },
    "MenuSidebar.DataGroup": {
        collapsible: false,
        defaultOpen: true,
        items: [
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
    },
    "MenuSidebar.ScreensGroup": {
        collapsible: false,
        items: [
            {
                title: "MenuSidebar.Overlay",
                href: "/overlay",
                icon: <Captions />
            },
            {
                title: "MenuSidebar.Stats",
                href: "/stats",
                icon: <ChartNoAxesCombined />
            },
        ],
    },
    "MenuSidebar.SettingsGroup": {
        collapsible: false,
        items: [
            {
                title: "MenuSidebar.DisplaySettings",
                href: "/settings/display",
                icon: <MonitorCog />
            },
            {
                title: "MenuSidebar.OverlaySettings",
                href: "/settings/overlay",
                icon: <Captions />
            }
        ],
    },
}

export const MenuSidebar = () => {
    const location = useLocation();

    const { open: sidebarOpen, toggleSidebar, isMobile } = useSidebar();
    const { translate } = useI18n();

    return (
        <Sidebar collapsible="icon">
            { !isMobile &&
                <SidebarHeader className="w-full items-end">
                    <Button onClick={toggleSidebar} variant="ghost" size="sm">
                        { sidebarOpen ? <ChevronLeft /> : <ChevronRight /> }
                    </Button>
                </SidebarHeader>
            }
            <SidebarContent>
                {
                    Object.keys(sidebarItems).map(groupName => {
                        const group = sidebarItems[groupName];

                        const GroupContent = () => (
                            <SidebarGroupContent>
                                <SidebarMenu>
                                    {group.items.map(item =>
                                        <SidebarMenuItem key={item.title}>
                                            <SidebarMenuButton asChild isActive={location.pathname.startsWith(item.href)} tooltip={translate(item.title)}>
                                                <Link to={item.href} onClick={() => isMobile && toggleSidebar()}>
                                                    {item.icon}
                                                    <span>{translate(item.title)}</span>
                                                </Link>
                                            </SidebarMenuButton>
                                        </SidebarMenuItem>
                                    )}
                                </SidebarMenu>
                            </SidebarGroupContent>
                        );

                        if(!!group.collapsible) {
                            return (
                                <Collapsible defaultOpen={group.defaultOpen} className="group/collapsible">
                                    <SidebarGroup key={groupName}>
                                        <SidebarGroupLabel asChild>
                                            <CollapsibleTrigger>
                                                {translate(groupName)}
                                                <ChevronDown className="ml-auto transition-transform group-data-[state=open]/collapsible:rotate-180" />
                                            </CollapsibleTrigger>
                                        </SidebarGroupLabel>
                                        <CollapsibleContent>
                                            <GroupContent />
                                        </CollapsibleContent>
                                    </SidebarGroup>
                                </Collapsible>
                            );
                        } else {
                            return (
                                <SidebarGroup key={groupName}>
                                    <SidebarGroupLabel>
                                        {translate(groupName)}
                                    </SidebarGroupLabel>
                                    <GroupContent />
                                </SidebarGroup>
                            );
                        }
                    })
                }
           </SidebarContent>
           <SidebarFooter>
                <SidebarMenu>
                    <LanguageMenu />
                    <ThemeMenu />
                    <SidebarMenuItem>
                        <SidebarMenuButton asChild tooltip={translate("MenuSidebar.Help")}>
                            <Link to="/help" onClick={() => isMobile && toggleSidebar()}>
                                <CircleHelp />
                                <span>{translate("MenuSidebar.Help")}</span>
                            </Link>
                        </SidebarMenuButton>
                    </SidebarMenuItem>
                </SidebarMenu>
            </SidebarFooter>
        </Sidebar>
    )
}