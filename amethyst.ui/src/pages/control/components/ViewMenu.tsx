import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuGroup, DropdownMenuItem, DropdownMenuRadioGroup, DropdownMenuRadioItem, DropdownMenuSeparator, DropdownMenuTrigger } from "@/components/ui"
import { DisplaySide, useUserSettings } from "@/hooks/UserSettings"
import { Check, Clock, Eye, Maximize2, NotebookPen, Play, Table, Tally5, UsersRound } from "lucide-react"

export const ViewMenu = () => {

    const userSettings = useUserSettings();

    const IconSpacer = () => (<span className="w-[16px]"></span>);

    const handleFullScreenClick = () => {
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen();
        } else if (document.exitFullscreen) {
            document.exitFullscreen();
        }
    }

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button size="icon" variant="ghost">
                    <Eye />
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56">
                <DropdownMenuGroup>
                    <DropdownMenuItem onClick={() => userSettings.setShowClockControls(!userSettings.showClockControls)}>
                        { userSettings.showClockControls ? (<Check />) : (<IconSpacer />) }
                        <Play />
                        <span>Clock controls</span>
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => userSettings.setShowScoreControls(!userSettings.showScoreControls)}>
                        { userSettings.showScoreControls ? (<Check />) : (<IconSpacer />) }
                        <Tally5 />
                        <span>Score controls</span>
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => userSettings.setShowStatsControls(!userSettings.showStatsControls)}>
                        { userSettings.showStatsControls ? (<Check />) : (<IconSpacer />) }
                        <NotebookPen />
                        <span>Stats controls</span>
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => userSettings.setShowLineupControls(!userSettings.showLineupControls)}>
                        { userSettings.showLineupControls ? (<Check />) : (<IconSpacer />) }
                        <UsersRound />
                        <span>Lineup controls</span>
                    </DropdownMenuItem>
                    <DropdownMenuItem onClick={() => userSettings.setShowClocks(!userSettings.showClocks)}>
                        { userSettings.showClocks ? (<Check />) : (<IconSpacer />) }
                        <Clock />
                        <span>Clocks</span>
                    </DropdownMenuItem>
                    <DropdownMenuItem>
                        <span className="w-[16px]"></span>
                        <Table />
                        <span>Score sheet</span>
                    </DropdownMenuItem>
                    <DropdownMenuSeparator />
                    <DropdownMenuRadioGroup value={userSettings.displaySide} onValueChange={v => userSettings.setDisplaySide(DisplaySide[v as keyof typeof DisplaySide])}>
                        <DropdownMenuRadioItem value={DisplaySide.Both}>Both teams</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value={DisplaySide.Home}>Home team</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value={DisplaySide.Away}>Away team</DropdownMenuRadioItem>
                    </DropdownMenuRadioGroup>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem onClick={handleFullScreenClick}>
                        <IconSpacer />
                        <Maximize2 />
                        Full screen
                    </DropdownMenuItem>
                </DropdownMenuGroup>
            </DropdownMenuContent>
        </DropdownMenu>
    )
}