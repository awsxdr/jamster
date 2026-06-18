import { Button, Collapsible, CollapsibleContent, CollapsibleTrigger, Sidebar, SidebarContent, SidebarFooter, SidebarGroup, SidebarGroupContent, SidebarGroupLabel, SidebarHeader, SidebarMenu, SidebarMenuBadge, SidebarMenuButton, SidebarMenuItem, useSidebar } from "@components/ui"
import { Captions, ChevronDown, ChevronLeft, ChevronRight, CircleHelp, ClipboardPenLine, ExternalLink, Grid3X3, Keyboard, List, MonitorCog, Shirt, TvMinimal, Users } from "lucide-react"
import { ReactNode, useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { LanguageMenu } from "./LanguageMenu";
import { ThemeMenu } from "./ThemeMenu";
import { useI18n } from "@/hooks/I18nHook";
import { useScreensApi } from "@/hooks";

type SidebarItem = {
    title: string;
    href: string;
    icon?: ReactNode;
    newTab?: boolean;
    isCustom?: boolean;
};

type SidebarGroup = {
    collapsible?: boolean;
    defaultOpen?: boolean;
    isCustom?: boolean;
    items: SidebarItem[];
}

type SidebarGroupList = Record<string, SidebarGroup>;

const defaultSidebarItems: SidebarGroupList = {
    "ControlGroup": {
        collapsible: true,
        defaultOpen: true,
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
        collapsible: true,
        defaultOpen: true,
        items: [
            {
                title: "ScoreboardDisplay",
                href: "/scoreboard",
                icon: <TvMinimal />,
                newTab: true,
            },
            {
                title: "Overlay",
                href: "/overlay",
                icon: <Captions />,
                newTab: true,
            },
            {
                title: "Penalties",
                href: "/penalties",
                icon: <Grid3X3 />,
                newTab: true,
            },
            {
                title: "DisplayManagement",
                href: "/clients",
                icon: <MonitorCog />,
            },
        ]
    },
    "DataGroup": {
        collapsible: true,
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
    // "ScreensGroup": {
    //     collapsible: false,
    //     items: [
    //         {
    //             title: "Stats",
    //             href: "/stats",
    //             icon: <ChartNoAxesCombined />
    //         },
    //     ],
    // },
    // "SettingsGroup": {
    //     collapsible: false,
    //     items: [
    //         {
    //             title: "Settings",
    //             href: "/settings",
    //             icon: <Settings />
    //         },
    //     ],
    // },
}

export const MenuSidebar = () => {
    const location = useLocation();

    const { open: sidebarOpen, toggleSidebar, isMobile } = useSidebar();
    const { translate } = useI18n({ prefix: "MenuSidebar." });
    const [sidebarItems, setSidebarItems] = useState(defaultSidebarItems);

    const { getScreens } = useScreensApi();

    useEffect(() => {
        getScreens().then(screens => {
            const categories = [...new Set(screens.map(s => s.category))];
            
            setSidebarItems({
                ...defaultSidebarItems,
                ...categories.reduce((result, category) => {
                    return {
                        ...result,
                        [category]: {
                            collapsible: true,
                            defaultOpen: false,
                            isCustom: !defaultSidebarItems[category],
                            ...defaultSidebarItems[category],
                            items: [
                                ...defaultSidebarItems[category]?.items ?? [],
                                ...screens.filter(s => s.category === category).map(s => ({
                                    title: s.name,
                                    href: s.url,
                                    newTab: s.ownTab,
                                    isCustom: true,
                                }))
                            ],
                        }
                    };
                }, {})
            })
        });
    }, []);

    return (
        <Sidebar collapsible="icon">
            { !isMobile &&
                <SidebarHeader className="w-full items-end">
                    <Button onClick={toggleSidebar} variant="ghost" size="sm">
                        { sidebarOpen ? <ChevronLeft /> : <ChevronRight /> }
                    </Button>
                </SidebarHeader>
            }
            <SidebarContent className="gap-0">
                {
                    Object.keys(sidebarItems).map(groupName => {
                        const group = sidebarItems[groupName];

                        const GroupContent = () => (
                            <SidebarGroupContent>
                                <SidebarMenu>
                                    {group.items.map(item =>
                                        <SidebarMenuItem key={item.title}>
                                            <SidebarMenuButton asChild isActive={location.pathname.startsWith(item.href)} tooltip={translate(item.title)}>
                                                <Link to={item.href} target={item.newTab ? "_blank" : "_self"} onClick={() => isMobile && toggleSidebar()}>
                                                    {item.icon}
                                                    <span>{translate(item.title, { fallback: item.isCustom ? item.title : undefined })}</span>
                                                </Link>
                                            </SidebarMenuButton>
                                            { item.newTab && (
                                                <SidebarMenuBadge>
                                                    <ExternalLink size="16" className="align-center" color="#ccc" />
                                                </SidebarMenuBadge>
                                            )}
                                        </SidebarMenuItem>
                                    )}
                                </SidebarMenu>
                            </SidebarGroupContent>
                        );

                        if(group.collapsible) {
                            return (
                                <Collapsible key={groupName} defaultOpen={group.defaultOpen} className="group/collapsible">
                                    <SidebarGroup className="p-1">
                                        <SidebarGroupLabel asChild>
                                            <CollapsibleTrigger>
                                                {translate(groupName, { fallback: group.isCustom ? groupName : undefined })}
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