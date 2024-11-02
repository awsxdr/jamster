import { Sidebar, SidebarContent, SidebarGroup, SidebarGroupContent, SidebarGroupLabel, SidebarHeader, SidebarMenu, SidebarMenuButton, SidebarMenuItem } from "@components/ui/sidebar"
import { Keyboard, TvMinimal } from "lucide-react"
import { useLocation } from "react-router-dom";

export const MenuSidebar = () => {
    const location = useLocation();

    return (
        <Sidebar collapsible="icon">
            <SidebarHeader />
            <SidebarContent>
                <SidebarGroup>
                    <SidebarGroupLabel>Main</SidebarGroupLabel>
                    <SidebarGroupContent>
                        <SidebarMenu>
                            <SidebarMenuItem key="Scoreboard display">
                                <SidebarMenuButton asChild>
                                    <a href="/scoreboard">
                                        <TvMinimal />
                                        <span>Scoreboard display</span>
                                    </a>
                                </SidebarMenuButton>
                            </SidebarMenuItem>
                            <SidebarMenuItem key="Scoreboard control">
                                <SidebarMenuButton asChild isActive={location.pathname === '/control'}>
                                    <a href="/control">
                                        <Keyboard />
                                        <span>Scoreboard control</span>
                                    </a>
                                </SidebarMenuButton>
                            </SidebarMenuItem>
                        </SidebarMenu>
                    </SidebarGroupContent>
                </SidebarGroup>
            </SidebarContent>
        </Sidebar>
    )
}