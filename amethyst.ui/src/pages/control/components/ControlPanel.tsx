import { MainControls } from "./MainControls"
import { TeamControls } from "./TeamControls"
import { Stage, TeamSide } from "@/types";
import { ClocksContainer } from "./ClocksContainer";
import { TimeoutTypePanel } from "./TimeoutTypePanel";
import { useGameStageState, useHasServerConnection } from "@/hooks";
import { DisplaySide, useUserSettings } from "@/hooks/UserSettings";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui";
import { WifiOff } from "lucide-react";

type ControlPanelProps = {
    gameId?: string;
    disabled?: boolean;
}

export const ControlPanel = ({ gameId, disabled }: ControlPanelProps) => {

    const hasConnection = useHasServerConnection();

    disabled = disabled || !hasConnection;

    const { stage, periodIsFinalized } = useGameStageState() ?? { stage: Stage.BeforeGame, periodIsFinalized: false };

    const { userSettings } = useUserSettings();

    const teamControlsDisabled = 
        disabled  || stage === Stage.AfterGame && periodIsFinalized ? true
        : stage === Stage.BeforeGame ? 'allExceptLineup'
        : false;

    return (
        <div className="flex flex-col gap-2 pt-2">
            { !hasConnection &&
                <Alert className="rounded-none" variant="destructive">
                    <WifiOff />
                    <AlertTitle className="ml-2">Connection lost</AlertTitle>
                    <AlertDescription className="ml-2">Connection to the server has been lost. Please check that the software is running and that your connection to the computer running it is working.</AlertDescription>
                </Alert>
            }
            { userSettings.showClockControls && <MainControls gameId={gameId} disabled={disabled} /> }
            { userSettings.showClockControls && <TimeoutTypePanel gameId={gameId} disabled={disabled} /> }
            { (userSettings.showLineupControls || userSettings.showScoreControls || userSettings.showStatsControls) && (
                <div className="w-full flex flex-wrap lg:flex-nowrap gap-2">
                    { userSettings.displaySide !== DisplaySide.Away && <TeamControls side={TeamSide.Home} gameId={gameId} disabled={teamControlsDisabled} /> }
                    { userSettings.displaySide !== DisplaySide.Home && <TeamControls side={TeamSide.Away} gameId={gameId} disabled={teamControlsDisabled} /> }
                </div>
            )}
            { userSettings.showClocks && <ClocksContainer /> }
        </div>
    )
}