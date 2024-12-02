import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuGroup, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui"
import { useUserSettings } from "@/hooks/UserSettings"
import { Check, Clock, Eye, NotebookPen, Play, Table, Tally5, UsersRound } from "lucide-react"

export const ViewMenu = () => {

    const userSettings = useUserSettings();

    const IconSpacer = () => (<span className="w-[16px]"></span>);

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
                </DropdownMenuGroup>
            </DropdownMenuContent>
        </DropdownMenu>
    )
}