import { MainControls } from "./MainControls"
import { TeamControls } from "./TeamControls"
import { ControlPanelViewConfiguration, DEFAULT_CONTROL_PANEL_VIEW_CONFIGURATION, DisplaySide, Stage, TeamSide } from "@/types";
import { ClocksContainer } from "./ClocksContainer";
import { TimeoutTypePanel } from "./TimeoutTypePanel";
import { useCurrentUserConfiguration, useGameStageState, useHasServerConnection } from "@/hooks";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui";
import { WifiOff } from "lucide-react";
import { TimeoutList } from "./TimeoutList";

type ControlPanelProps = {
    gameId?: string;
    disabled?: boolean;
}

export const ControlPanel = ({ gameId, disabled }: ControlPanelProps) => {

    const hasConnection = useHasServerConnection();

    disabled = disabled || !hasConnection;

    const { stage, periodIsFinalized } = useGameStageState() ?? { stage: Stage.BeforeGame, periodIsFinalized: false };

    const { configuration: viewConfiguration } = useCurrentUserConfiguration<ControlPanelViewConfiguration>("ControlPanelViewConfiguration", DEFAULT_CONTROL_PANEL_VIEW_CONFIGURATION);

    const teamControlsDisabled = 
        disabled  || stage === Stage.AfterGame && periodIsFinalized ? true
        : stage === Stage.BeforeGame ? 'allExceptLineup'
        : false;

    return (
        <div className="flex flex-col gap-2 pt-2 pb-2">
            { !hasConnection &&
                <Alert className="rounded-none" variant="destructive">
                    <WifiOff />
                    <AlertTitle className="ml-2">Connection lost</AlertTitle>
                    <AlertDescription className="ml-2">Connection to the server has been lost. Please check that the software is running and that your connection to the computer running it is working.</AlertDescription>
                </Alert>
            }
            { viewConfiguration.showClockControls && <MainControls gameId={gameId} disabled={disabled} /> }
            { viewConfiguration.showClockControls && <TimeoutTypePanel gameId={gameId} disabled={disabled} /> }
            { (viewConfiguration.showLineupControls || viewConfiguration.showScoreControls || viewConfiguration.showStatsControls) && (
                <div className="w-full flex flex-wrap xl:flex-nowrap gap-2">
                    { viewConfiguration.displaySide !== DisplaySide.Away && 
                        <TeamControls 
                            side={TeamSide.Home} 
                            gameId={gameId} 
                            disabled={teamControlsDisabled} 
                            className={viewConfiguration.displaySide == DisplaySide.Both ? "xl:w-1/2" : ""} 
                        /> 
                    }
                    { viewConfiguration.displaySide !== DisplaySide.Home && 
                        <TeamControls 
                            side={TeamSide.Away} 
                            gameId={gameId} 
                            disabled={teamControlsDisabled} 
                            className={viewConfiguration.displaySide == DisplaySide.Both ? "xl:w-1/2" : ""} 
                        /> 
                    }
                </div>
            )}
            { viewConfiguration.showClocks && <ClocksContainer /> }
            { viewConfiguration.showTimeoutList &&
                <TimeoutList gameId={gameId} displaySide={viewConfiguration.displaySide} />
            }
        </div>
    )
}