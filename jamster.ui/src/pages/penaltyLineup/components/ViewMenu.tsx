import { Eye, Maximize2 } from "lucide-react"
import { Button, DropdownMenu, DropdownMenuContent, DropdownMenuGroup, DropdownMenuItem, DropdownMenuLabel, DropdownMenuRadioGroup, DropdownMenuRadioItem, DropdownMenuSeparator, DropdownMenuTrigger } from "@/components/ui"
import { useI18n, useTeamDetailsState, useWakeLock } from "@/hooks";
import { DisplaySide, GameTeam, TeamSide } from "@/types";
import { useMemo } from "react";

export type PltDisplayType = "None" | "Both" | "Penalties" | "Lineup";
export type BoxDisplayType = "None" | "Both" | "Jammers" | "Blockers";

type ViewMenuProps = {
    displaySide: DisplaySide;
    pltDisplayType: PltDisplayType;
    boxDisplayType: BoxDisplayType;
    onDisplaySideChanged: (side: DisplaySide) => void;
    onPltDisplayTypeChanged: (displayTye: PltDisplayType) => void;
    onBoxDisplayTypechanged: (displayType: BoxDisplayType) => void;
    disabled?: boolean;
}

export const ViewMenu = ({ 
    displaySide, 
    pltDisplayType, 
    boxDisplayType,
    onDisplaySideChanged, 
    onPltDisplayTypeChanged, 
    onBoxDisplayTypechanged, 
    disabled 
}: ViewMenuProps) => {

    const { acquireWakeLock, releaseWakeLock } = useWakeLock();
    const { translate } = useI18n({ prefix: "PenaltyLineup.ViewMenu." });

    const { team: homeTeam } = useTeamDetailsState(TeamSide.Home) ?? { };
    const { team: awayTeam } = useTeamDetailsState(TeamSide.Away) ?? { };
    
    const handleFullScreenClick = () => {
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen();
            acquireWakeLock();
        } else if (document.exitFullscreen) {
            document.exitFullscreen();
            releaseWakeLock();
        }
    }

    const getTeamName = (team: GameTeam | undefined) => {
        if(!team) {
            return "";
        }

        return team.names["controls"] || team.names["color"] ||  team.names["team"] || team.names["league"] || "";
    }

    const homeTeamName = useMemo(() => getTeamName(homeTeam), [homeTeam]);
    const awayTeamName = useMemo(() => getTeamName(awayTeam), [awayTeam]);

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button id="PenaltyLineup.ViewMenu" size="icon" variant="ghost" disabled={disabled}>
                    <Eye />
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent className="w-56">
                <DropdownMenuLabel>{translate("Team")}</DropdownMenuLabel>
                <DropdownMenuRadioGroup value={displaySide} onValueChange={v => onDisplaySideChanged(DisplaySide[v as keyof typeof DisplaySide])}>
                    <DropdownMenuRadioItem id="PenaltyLineup.ViewMenu.Team.Both" disabled={disabled} value={DisplaySide.Both}>{translate("Teams.Both")}</DropdownMenuRadioItem>
                    <DropdownMenuRadioItem id="PenaltyLineup.ViewMenu.Team.Home" disabled={disabled} value={DisplaySide.Home}>{homeTeamName}</DropdownMenuRadioItem>
                    <DropdownMenuRadioItem id="PenaltyLineup.ViewMenu.Team.Away" disabled={disabled} value={DisplaySide.Away}>{awayTeamName}</DropdownMenuRadioItem>
                </DropdownMenuRadioGroup>
                <DropdownMenuSeparator />
                <DropdownMenuGroup>
                    <DropdownMenuLabel>{translate("Plt")}</DropdownMenuLabel>
                    <DropdownMenuRadioGroup value={pltDisplayType} onValueChange={v => onPltDisplayTypeChanged(v as PltDisplayType)}>
                        <DropdownMenuRadioItem value="None">{translate("Plt.None")}</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value="Penalties">{translate("Plt.Penalties")}</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value="Lineup">{translate("Plt.Lineup")}</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value="Both">{translate("Plt.Both")}</DropdownMenuRadioItem>
                    </DropdownMenuRadioGroup>
                </DropdownMenuGroup>
                <DropdownMenuSeparator />
                <DropdownMenuGroup>
                    <DropdownMenuLabel>{translate("Box")}</DropdownMenuLabel>
                    <DropdownMenuRadioGroup value={boxDisplayType} onValueChange={v => onBoxDisplayTypechanged(v as BoxDisplayType)}>
                        <DropdownMenuRadioItem value="None">{translate("Box.None")}</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value="Jammers">{translate("Box.Jammers")}</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value="Blockers">{translate("Box.Blockers")}</DropdownMenuRadioItem>
                        <DropdownMenuRadioItem value="Both">{translate("Box.Both")}</DropdownMenuRadioItem>
                    </DropdownMenuRadioGroup>
                </DropdownMenuGroup>
                <DropdownMenuSeparator />
                <DropdownMenuGroup>
                    <DropdownMenuItem disabled={disabled} onClick={handleFullScreenClick}>
                        <Maximize2 />
                        {translate("FullScreen")}
                    </DropdownMenuItem>
                </DropdownMenuGroup>
            </DropdownMenuContent>
        </DropdownMenu>
    )
}