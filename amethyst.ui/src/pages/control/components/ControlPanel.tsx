import { MainControls } from "./MainControls"
import { TeamControls } from "./TeamControls"
import { ControlPanelViewConfiguration, DEFAULT_CONTROL_PANEL_VIEW_CONFIGURATION, DisplaySide, Stage, TeamSide } from "@/types";
import { ClocksContainer } from "./ClocksContainer";
import { TimeoutTypePanel } from "./TimeoutTypePanel";
import { useCurrentUserConfiguration, useGameStageState, useHasServerConnection } from "@/hooks";
import { TimeoutList } from "./TimeoutList";
import { ScoreSheetContainer } from "./statsSheet/ScoreSheetContainer";
import { ConnectionLostAlert } from "@/components/ConnectionLostAlert";

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
            <ConnectionLostAlert />
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
            { gameId && viewConfiguration.showClocks && <ClocksContainer gameId={gameId} /> }
            { viewConfiguration.showTimeoutList &&
                <TimeoutList gameId={gameId} displaySide={viewConfiguration.displaySide} />
            }
            { viewConfiguration.showScoreSheet && gameId &&
                <ScoreSheetContainer gameId={gameId} displaySide={viewConfiguration.displaySide} />
            }
        </div>
    )
}