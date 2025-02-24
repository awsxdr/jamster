import { Eye, Maximize2 } from "lucide-react"
import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuGroup, DropdownMenuItem, DropdownMenuRadioGroup, DropdownMenuRadioItem, DropdownMenuSeparator, DropdownMenuTrigger } from "@/components/ui"
import { useI18n, useWakeLock } from "@/hooks";
import { DisplaySide } from "@/types";

type ViewMenuProps = {
    displaySide: DisplaySide;
    onDisplaySideChanged: (side: DisplaySide) => void;
    disabled?: boolean;
}

export const ViewMenu = ({ displaySide, onDisplaySideChanged, disabled }: ViewMenuProps) => {

    const IconSpacer = () => (<span className="w-[16px]"></span>);
    const { acquireWakeLock, releaseWakeLock } = useWakeLock();
    const { translate } = useI18n();
    
    const handleFullScreenClick = () => {
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen();
            acquireWakeLock();
        } else if (document.exitFullscreen) {
            document.exitFullscreen();
            releaseWakeLock();
        }
    }

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button size="icon" variant="ghost" disabled={disabled}>
                    <Eye />
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56">
                <DropdownMenuRadioGroup value={displaySide} onValueChange={v => onDisplaySideChanged(DisplaySide[v as keyof typeof DisplaySide])}>
                    <DropdownMenuRadioItem disabled={disabled} value={DisplaySide.Both}>{translate("ViewMenu.BothTeams")}</DropdownMenuRadioItem>
                    <DropdownMenuRadioItem disabled={disabled} value={DisplaySide.Home}>{translate("ViewMenu.HomeTeam")}</DropdownMenuRadioItem>
                    <DropdownMenuRadioItem disabled={disabled} value={DisplaySide.Away}>{translate("ViewMenu.AwayTeam")}</DropdownMenuRadioItem>
                </DropdownMenuRadioGroup>
                <DropdownMenuSeparator />
                <DropdownMenuGroup>
                    <DropdownMenuItem disabled={disabled} onClick={handleFullScreenClick}>
                        <IconSpacer />
                        <Maximize2 />
                        {translate("ViewMenu.FullScreen")}
                    </DropdownMenuItem>
                </DropdownMenuGroup>
            </DropdownMenuContent>
        </DropdownMenu>
    )
}