import { Check, Clock, Eye, Maximize2, NotebookPen, Package, Play, Table, Tally5, Timer, UsersRound } from "lucide-react"
import { DropdownMenu, DropdownMenuContent, DropdownMenuGroup, DropdownMenuItem, DropdownMenuLabel, DropdownMenuRadioGroup, DropdownMenuRadioItem, DropdownMenuSeparator, DropdownMenuSub, DropdownMenuSubContent, DropdownMenuSubTrigger, DropdownMenuTrigger } from "@/components/ui"
import { useCurrentUserConfiguration, useI18n, useIsMobile, useWakeLock } from "@/hooks";
import { ControlPanelViewConfiguration, DEFAULT_CONTROL_PANEL_VIEW_CONFIGURATION, DisplaySide } from "@/types";
import { TooltipButton } from "@/components";

type ViewMenuProps = {
    disabled?: boolean;
}

export const ViewMenu = ({ disabled }: ViewMenuProps) => {

    const { configuration: viewConfiguration, setConfiguration: setViewConfiguration } = useCurrentUserConfiguration<ControlPanelViewConfiguration>("ControlPanelViewConfiguration", DEFAULT_CONTROL_PANEL_VIEW_CONFIGURATION);

    const IconSpacer = () => (<span className="w-[16px]"></span>);
    const isMobile = useIsMobile();
    const { acquireWakeLock, releaseWakeLock } = useWakeLock();
    const { translate } = useI18n({ prefix: "ScoreboardControl.ViewMenu." });
    
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
        setViewConfiguration({
            ...viewConfiguration,
            showClockControls: true,
            showScoreControls: true,
            showStatsControls: true,
            showLineupControls: true,
            showClocks: true,
            showTimeoutList: false,
            showScoreSheet: true,
            displaySide: DisplaySide.Both,
        });
    }

    const handleScorekeeperPresetClick = () => {
        setViewConfiguration({
            ...viewConfiguration,
            showClockControls: false,
            showScoreControls: true,
            showStatsControls: true,
            showLineupControls: true,
            showClocks: true,
            showTimeoutList: false,
            showScoreSheet: true,
        });
    }

    const handleJamTimerPresetClick = () => {
        setViewConfiguration({
            ...viewConfiguration,
            showClockControls: true,
            showScoreControls: false,
            showStatsControls: false,
            showLineupControls: false,
            showClocks: true,
            showTimeoutList: true,
            showScoreSheet: false,
            displaySide: DisplaySide.Both,
        });
    }

    const toggleShowClockControls = () => setViewConfiguration({
        ...viewConfiguration,
        showClockControls: !viewConfiguration.showClockControls,
    });

    const toggleShowScoreControls = () => setViewConfiguration({
        ...viewConfiguration,
        showScoreControls: !viewConfiguration.showScoreControls,
    });

    const toggleShowStatsControls = () => setViewConfiguration({
        ...viewConfiguration,
        showStatsControls: !viewConfiguration.showStatsControls,
    });

    const toggleShowLineupControls = () => setViewConfiguration({
        ...viewConfiguration,
        showLineupControls: !viewConfiguration.showLineupControls,
    });

    const toggleShowClocks = () => setViewConfiguration({
        ...viewConfiguration,
        showClocks: !viewConfiguration.showClocks,
    });

    const toggleShowTimeouts = () => setViewConfiguration({
        ...viewConfiguration,
        showTimeoutList: !viewConfiguration.showTimeoutList,
    });

    const toggleShowScoreSheet = () => setViewConfiguration({
        ...viewConfiguration,
        showScoreSheet: !viewConfiguration.showScoreSheet,
    });

    const setDisplaySide = (displaySide: DisplaySide) => setViewConfiguration({
        ...viewConfiguration,
        displaySide
    });

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <TooltipButton description={translate("Tooltip")} size="icon" variant="ghost" disabled={disabled}>
                    <Eye />
                </TooltipButton>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56">
                { isMobile ? (
                    <DropdownMenuGroup>
                        <DropdownMenuLabel>{translate("Presets")}</DropdownMenuLabel>
                        <DropdownMenuItem disabled={disabled} onClick={handleScoreboardOperatorPresetClick}>
                            <IconSpacer />
                            {translate("ScoreboardPreset")}
                        </DropdownMenuItem>
                        <DropdownMenuItem disabled={disabled} onClick={handleScorekeeperPresetClick}>
                            <IconSpacer />
                            {translate("ScorekeeperPreset")}
                        </DropdownMenuItem>
                        <DropdownMenuItem disabled={disabled} onClick={handleJamTimerPresetClick}>
                            <IconSpacer />
                            {translate("JamTimerPreset")}
                        </DropdownMenuItem>
                    </DropdownMenuGroup>
                ) : (
                    <DropdownMenuSub>
                        <DropdownMenuSubTrigger><IconSpacer /><Package />{translate("Presets")}</DropdownMenuSubTrigger>
                        <DropdownMenuSubContent className="w-56">
                            <DropdownMenuItem disabled={disabled} onClick={handleScoreboardOperatorPresetClick}>
                                {translate("ScoreboardPreset")}
                            </DropdownMenuItem>
                            <DropdownMenuItem disabled={disabled} onClick={handleScorekeeperPresetClick}>
                                {translate("ScorekeeperPreset")}
                            </DropdownMenuItem>
                            <DropdownMenuItem disabled={disabled} onClick={handleJamTimerPresetClick}>
                                {translate("JamTimerPreset")}
                            </DropdownMenuItem>
                        </DropdownMenuSubContent>
                    </DropdownMenuSub>
                )}
                <DropdownMenuSeparator />
                <DropdownMenuGroup>
                    <DropdownMenuLabel>{translate("Controls")}</DropdownMenuLabel>
                    <DropdownMenuItem disabled={disabled} onClick={toggleShowClockControls}>
                        { viewConfiguration.showClockControls ? (<Check />) : (<IconSpacer />) }
                        <Play />
                        {translate("ClockControls")}
                    </DropdownMenuItem>
                    <DropdownMenuItem disabled={disabled} onClick={toggleShowScoreControls}>
                        { viewConfiguration.showScoreControls ? (<Check />) : (<IconSpacer />) }
                        <Tally5 />
                        {translate("ScoreControls")}
                    </DropdownMenuItem>
                    <DropdownMenuItem disabled={disabled} onClick={toggleShowStatsControls}>
                        { viewConfiguration.showStatsControls ? (<Check />) : (<IconSpacer />) }
                        <NotebookPen />
                        {translate("StatsControls")}
                    </DropdownMenuItem>
                    <DropdownMenuItem disabled={disabled} onClick={toggleShowLineupControls}>
                        { viewConfiguration.showLineupControls ? (<Check />) : (<IconSpacer />) }
                        <UsersRound />
                        {translate("LineupControls")}
                    </DropdownMenuItem>
                    <DropdownMenuItem disabled={disabled} onClick={toggleShowClocks}>
                        { viewConfiguration.showClocks ? (<Check />) : (<IconSpacer />) }
                        <Clock />
                        {translate("Clocks")}
                    </DropdownMenuItem>
                    <DropdownMenuItem disabled={disabled} onClick={toggleShowTimeouts}>
                        { viewConfiguration.showTimeoutList ? (<Check />) : (<IconSpacer />) }
                        <Timer />
                        {translate("Timeouts")}
                    </DropdownMenuItem>
                    <DropdownMenuItem disabled={disabled} onClick={toggleShowScoreSheet}>
                        { viewConfiguration.showScoreSheet ? (<Check />) : (<IconSpacer />) }
                        <Table />
                        {translate("ScoreSheet")}
                    </DropdownMenuItem>
                </DropdownMenuGroup>
                <DropdownMenuSeparator />
                <DropdownMenuRadioGroup value={viewConfiguration.displaySide} onValueChange={v => setDisplaySide(DisplaySide[v as keyof typeof DisplaySide])}>
                    <DropdownMenuLabel>{translate("Teams")}</DropdownMenuLabel>
                    <DropdownMenuRadioItem disabled={disabled} value={DisplaySide.Both}>{translate("BothTeams")}</DropdownMenuRadioItem>
                    <DropdownMenuRadioItem disabled={disabled} value={DisplaySide.Home}>{translate("HomeTeam")}</DropdownMenuRadioItem>
                    <DropdownMenuRadioItem disabled={disabled} value={DisplaySide.Away}>{translate("AwayTeam")}</DropdownMenuRadioItem>
                </DropdownMenuRadioGroup>
                <DropdownMenuSeparator />
                <DropdownMenuGroup>
                    <DropdownMenuItem disabled={disabled} onClick={handleFullScreenClick}>
                        <IconSpacer />
                        <Maximize2 />
                        {translate("FullScreen")}
                    </DropdownMenuItem>
                </DropdownMenuGroup>
            </DropdownMenuContent>
        </DropdownMenu>
    )
}