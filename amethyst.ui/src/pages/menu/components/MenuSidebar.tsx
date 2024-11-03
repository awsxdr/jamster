import { Button } from "@/components/ui/button";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu";
import { useTheme } from "@/hooks/ThemeHook";
import { Sidebar, SidebarContent, SidebarFooter, SidebarGroup, SidebarGroupContent, SidebarGroupLabel, SidebarHeader, SidebarMenu, SidebarMenuButton, SidebarMenuItem, SidebarTrigger, useSidebar } from "@components/ui/sidebar"
import { ChevronLeft, ChevronRight, ChevronUp, Keyboard, List, Palette, TvMinimal, Users } from "lucide-react"
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

    const { setTheme } = useTheme();

    const { open: sidebarOpen, toggleSidebar } = useSidebar();
    

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
           <SidebarFooter>
                <SidebarMenu>
                    <SidebarMenuItem>
                        <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                                <SidebarMenuButton>
                                    <Palette /> Choose theme
                                    <ChevronUp className="ml-auto" />
                                </SidebarMenuButton>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent
                                side="top"
                                className="w-[--radix-popper-anchor-width]"
                            >
                                <DropdownMenuItem onClick={() => setTheme("light")}><span>Light</span></DropdownMenuItem>
                                <DropdownMenuItem onClick={() => setTheme("dark")}><span>Dark</span></DropdownMenuItem>
                                <DropdownMenuItem onClick={() => setTheme("system")}><span>Match system default</span></DropdownMenuItem>
                            </DropdownMenuContent>
                        </DropdownMenu>
                    </SidebarMenuItem>
                </SidebarMenu>
            </SidebarFooter>
        </Sidebar>
    )
}