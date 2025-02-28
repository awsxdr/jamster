import { Eye, Maximize2 } from "lucide-react"
import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuGroup, DropdownMenuItem, DropdownMenuLabel, DropdownMenuRadioGroup, DropdownMenuRadioItem, DropdownMenuSeparator, DropdownMenuTrigger } from "@/components/ui"
import { useI18n, useWakeLock } from "@/hooks";
import { DisplaySide } from "@/types";

export type PltDisplayType = "None" | "Both" | "Penalties" | "Lineup";

type ViewMenuProps = {
    displaySide: DisplaySide;
    pltDisplayType: PltDisplayType;
    onDisplaySideChanged: (side: DisplaySide) => void;
    onPltDisplayTypeChanged: (displayTye: PltDisplayType) => void;
    disabled?: boolean;
}

export const ViewMenu = ({ displaySide, pltDisplayType, onDisplaySideChanged, onPltDisplayTypeChanged, disabled }: ViewMenuProps) => {

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
                <DropdownMenuLabel>Team</DropdownMenuLabel>
                <DropdownMenuRadioGroup value={displaySide} onValueChange={v => onDisplaySideChanged(DisplaySide[v as keyof typeof DisplaySide])}>
                    <DropdownMenuRadioItem disabled={disabled} value={DisplaySide.Both}>{translate("ViewMenu.BothTeams")}</DropdownMenuRadioItem>
                    <DropdownMenuRadioItem disabled={disabled} value={DisplaySide.Home}>{translate("ViewMenu.HomeTeam")}</DropdownMenuRadioItem>
                    <DropdownMenuRadioItem disabled={disabled} value={DisplaySide.Away}>{translate("ViewMenu.AwayTeam")}</DropdownMenuRadioItem>
                </DropdownMenuRadioGroup>
                <DropdownMenuSeparator />
                <DropdownMenuGroup>
                    <DropdownMenuLabel>Penalty/lineup entry</DropdownMenuLabel>
                    <DropdownMenuRadioGroup value={pltDisplayType} onValueChange={v => onPltDisplayTypeChanged?.(v as PltDisplayType)}>
                        <DropdownMenuRadioItem value="None">None</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value="Penalties">Penalties</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value="Lineup">Lineup</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value="Both">Both</DropdownMenuRadioItem>
                    </DropdownMenuRadioGroup>
                </DropdownMenuGroup>
                <DropdownMenuSeparator />
                <DropdownMenuGroup>
                    <DropdownMenuLabel>Box timing</DropdownMenuLabel>
                    <DropdownMenuRadioGroup value="None">
                        <DropdownMenuRadioItem value="None">None</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value="Jammers">Jammers</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value="Blockers">Blockers</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value="Both">Both</DropdownMenuRadioItem>
                    </DropdownMenuRadioGroup>
                </DropdownMenuGroup>
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