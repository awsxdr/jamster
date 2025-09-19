import { Button, SidebarTrigger, useSidebar } from "./ui"

type MobileSidebarTriggerProps = React.ComponentProps<typeof Button>;

export const MobileSidebarTrigger = (props: MobileSidebarTriggerProps) => {
    const { isMobile } = useSidebar();

    return isMobile ? <SidebarTrigger {...props} /> : <></>
}