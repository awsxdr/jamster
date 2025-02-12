import { Button, Collapsible, CollapsibleContent, CollapsibleTrigger, Sidebar, SidebarContent, SidebarFooter, SidebarGroup, SidebarGroupContent, SidebarGroupLabel, SidebarHeader, SidebarMenu, SidebarMenuButton, SidebarMenuItem, useSidebar } from "@components/ui"
import { Captions, ChartNoAxesCombined, ChevronDown, ChevronLeft, ChevronRight, CircleHelp, ClipboardPenLine, Grid3X3, Keyboard, List, Settings, Shirt, TvMinimal, Users } from "lucide-react"
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
    "ControlGroup": {
        items: [
            {
                title: "ScoreboardControl",
                href: "/sbo",
                icon: <Keyboard />
            },
            {
                title: "PenaltyLineup",
                href: "/plt",
                icon: <ClipboardPenLine />
            },
        ],
    },
    "DisplayGroup": {
        items: [
            {
                title: "ScoreboardDisplay",
                href: "/scoreboard",
                icon: <TvMinimal />
            },
            {
                title: "Overlay",
                href: "/overlay",
                icon: <Captions />
            },
            {
                title: "Penalties",
                href: "/penalties",
                icon: <Grid3X3 />
            }
        ]
    },
    "DataGroup": {
        collapsible: false,
        defaultOpen: true,
        items: [
            {
                title: "Games",
                href: "/games",
                icon: <List />
            },
            {
                title: "Teams",
                href: "/teams",
                icon: <Shirt />
            },
            {
                title: "Users",
                href: "/users",
                icon: <Users />
            },
        ],
    },
    "ScreensGroup": {
        collapsible: false,
        items: [
            {
                title: "Stats",
                href: "/stats",
                icon: <ChartNoAxesCombined />
            },
        ],
    },
    "SettingsGroup": {
        collapsible: false,
        items: [
            {
                title: "Settings",
                href: "/settings",
                icon: <Settings />
            },
        ],
    },
}

export const MenuSidebar = () => {
    const location = useLocation();

    const { open: sidebarOpen, toggleSidebar, isMobile } = useSidebar();
    const { translate } = useI18n({ prefix: "MenuSidebar." });

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
                        <SidebarMenuButton asChild tooltip={translate("Help")}>
                            <Link to="/help" onClick={() => isMobile && toggleSidebar()}>
                                <CircleHelp />
                                <span>{translate("Help")}</span>
                            </Link>
                        </SidebarMenuButton>
                    </SidebarMenuItem>
                </SidebarMenu>
            </SidebarFooter>
        </Sidebar>
    )
}