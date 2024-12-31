import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuGroup, DropdownMenuItem, DropdownMenuLabel, DropdownMenuRadioGroup, DropdownMenuRadioItem, DropdownMenuSeparator, DropdownMenuSub, DropdownMenuSubContent, DropdownMenuSubTrigger, DropdownMenuTrigger } from "@/components/ui"
import { useI18n } from "@/hooks";
import { useIsMobile } from "@/hooks/use-mobile";
import { DisplaySide, useUserSettings } from "@/hooks/UserSettings"
import { useWakeLock } from "@/hooks/WakeLock";
import { Check, Clock, Eye, Maximize2, NotebookPen, Package, Play, Table, Tally5, Timer, UsersRound } from "lucide-react"

type ViewMenuProps = {
    disabled?: boolean;
}

export const ViewMenu = ({ disabled }: ViewMenuProps) => {

    const { userSettings, setUserSettings } = useUserSettings();

    const IconSpacer = () => (<span className="w-[16px]"></span>);
    const isMobile = useIsMobile();
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

    const handleScoreboardOperatorPresetClick = () => {
        setUserSettings(current => ({
            ...current,
            showClockControls: true,
            showScoreControls: true,
            showStatsControls: true,
            showLineupControls: true,
            showClocks: true,
            showTimeoutList: true,
            displaySide: DisplaySide.Both,
        }));
    }

    const handleScorekeeperPresetClick = () => {
        setUserSettings(current => ({
            ...current,
            showClockControls: false,
            showScoreControls: true,
            showStatsControls: true,
            showLineupControls: true,
            showClocks: true,
            showTimeoutList: false,
        }));
    }

    const handleJamTimerPresetClick = () => {
        setUserSettings(current => ({
            ...current,
            showClockControls: true,
            showScoreControls: false,
            showStatsControls: false,
            showLineupControls: false,
            showClocks: true,
            showTimeoutList: true,
            displaySide: DisplaySide.Both,
        }));
    }

    const toggleShowClockControls = () => setUserSettings(current => ({
        ...current,
        showClockControls: !current.showClockControls,
    }));

    const toggleShowScoreControls = () => setUserSettings(current => ({
        ...current,
        showScoreControls: !current.showScoreControls,
    }));

    const toggleShowStatsControls = () => setUserSettings(current => ({
        ...current,
        showStatsControls: !current.showStatsControls,
    }));

    const toggleShowLineupControls = () => setUserSettings(current => ({
        ...current,
        showLineupControls: !current.showLineupControls,
    }));

    const toggleShowClocks = () => setUserSettings(current => ({
        ...current,
        showClocks: !current.showClocks,
    }));

    const toggleShowTimeouts = () => setUserSettings(current => ({
        ...current,
        showTimeoutList: !current.showTimeoutList,
    }));

    const setDisplaySide = (displaySide: DisplaySide) => setUserSettings(current => ({
        ...current,
        displaySide
    }));

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button size="icon" variant="ghost" disabled={disabled}>
                    <Eye />
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56">
                { isMobile ? (
                    <DropdownMenuGroup>
                        <DropdownMenuLabel>{translate("ViewMenu.Presets")}</DropdownMenuLabel>
                        <DropdownMenuItem disabled={disabled} onClick={handleScoreboardOperatorPresetClick}>
                            <IconSpacer />
                            {translate("ViewMenu.ScoreboardPreset")}
                        </DropdownMenuItem>
                        <DropdownMenuItem disabled={disabled} onClick={handleScorekeeperPresetClick}>
                            <IconSpacer />
                            {translate("ViewMenu.ScorekeeperPreset")}
                        </DropdownMenuItem>
                        <DropdownMenuItem disabled={disabled} onClick={handleJamTimerPresetClick}>
                            <IconSpacer />
                            {translate("ViewMenu.JamTimerPreset")}
                        </DropdownMenuItem>
                    </DropdownMenuGroup>
                ) : (
                    <DropdownMenuSub>
                        <DropdownMenuSubTrigger><IconSpacer /><Package />{translate("ViewMenu.Presets")}</DropdownMenuSubTrigger>
                        <DropdownMenuSubContent className="w-56">
                            <DropdownMenuItem disabled={disabled} onClick={handleScoreboardOperatorPresetClick}>
                                {translate("ViewMenu.ScoreboardPreset")}
                            </DropdownMenuItem>
                            <DropdownMenuItem disabled={disabled} onClick={handleScorekeeperPresetClick}>
                                {translate("ViewMenu.ScorekeeperPreset")}
                            </DropdownMenuItem>
                            <DropdownMenuItem disabled={disabled} onClick={handleJamTimerPresetClick}>
                                {translate("ViewMenu.JamTimerPreset")}
                            </DropdownMenuItem>
                        </DropdownMenuSubContent>
                    </DropdownMenuSub>
                )}
                <DropdownMenuSeparator />
                <DropdownMenuGroup>
                    <DropdownMenuItem disabled={disabled} onClick={toggleShowClockControls}>
                        { userSettings.showClockControls ? (<Check />) : (<IconSpacer />) }
                        <Play />
                        {translate("ViewMenu.ClockControls")}
                    </DropdownMenuItem>
                    <DropdownMenuItem disabled={disabled} onClick={toggleShowScoreControls}>
                        { userSettings.showScoreControls ? (<Check />) : (<IconSpacer />) }
                        <Tally5 />
                        {translate("ViewMenu.ScoreControls")}
                    </DropdownMenuItem>
                    <DropdownMenuItem disabled={disabled} onClick={toggleShowStatsControls}>
                        { userSettings.showStatsControls ? (<Check />) : (<IconSpacer />) }
                        <NotebookPen />
                        {translate("ViewMenu.StatsControls")}
                    </DropdownMenuItem>
                    <DropdownMenuItem disabled={disabled} onClick={toggleShowLineupControls}>
                        { userSettings.showLineupControls ? (<Check />) : (<IconSpacer />) }
                        <UsersRound />
                        {translate("ViewMenu.LineupControls")}
                    </DropdownMenuItem>
                    <DropdownMenuItem disabled={disabled} onClick={toggleShowClocks}>
                        { userSettings.showClocks ? (<Check />) : (<IconSpacer />) }
                        <Clock />
                        {translate("ViewMenu.Clocks")}
                    </DropdownMenuItem>
                    <DropdownMenuItem disabled={disabled} onClick={toggleShowTimeouts}>
                        { userSettings.showTimeoutList ? (<Check />) : (<IconSpacer />) }
                        <Timer />
                        {translate("ViewMenu.Timeouts")}
                    </DropdownMenuItem>
                    <DropdownMenuItem disabled={disabled}>
                        <span className="w-[16px]"></span>
                        <Table />
                        {translate("ViewMenu.ScoreSheet")}
                    </DropdownMenuItem>
                </DropdownMenuGroup>
                <DropdownMenuSeparator />
                <DropdownMenuRadioGroup value={userSettings.displaySide} onValueChange={v => setDisplaySide(DisplaySide[v as keyof typeof DisplaySide])}>
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