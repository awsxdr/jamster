import { Sidebar, SidebarContent, SidebarGroup, SidebarGroupContent, SidebarGroupLabel, SidebarHeader, SidebarMenu, SidebarMenuButton, SidebarMenuItem } from "@components/ui/sidebar"
import { Keyboard, List, TvMinimal, Users } from "lucide-react"
import { ReactNode } from "react";
import { useLocation } from "react-router-dom";

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
            title: "Scoreboard display",
            href: "/scoreboard",
            icon: <TvMinimal />
        },
        {
            title: "Scoreboard control",
            href: "/control",
            icon: <Keyboard />
        }
    ],
    "Data": [
        {
            title: "Teams",
            href: "/teams",
            icon: <Users />
        },
        {
            title: "Games",
            href: "/games",
            icon: <List />
        }
    ],
}

export const MenuSidebar = () => {
    const location = useLocation();

    return (
        <Sidebar collapsible="icon">
            <SidebarHeader />
            <SidebarContent>
                {
                    Object.keys(sidebarItems).map(groupName => {
                        const items = sidebarItems[groupName];

                        return (
                            <SidebarGroup key={groupName}>
                                <SidebarGroupLabel>{groupName}</SidebarGroupLabel>
                                <SidebarGroupContent>
                                    <SidebarMenu>
                                        {items.map(item =>
                                            <SidebarMenuItem key={item.title}>
                                                <SidebarMenuButton asChild isActive={location.pathname === item.href}>
                                                    <a href={item.href}>
                                                        {item.icon}
                                                        <span>{item.title}</span>
                                                    </a>
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
        </Sidebar>
    )
}