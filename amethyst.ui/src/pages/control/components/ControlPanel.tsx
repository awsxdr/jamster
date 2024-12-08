import { MainControls } from "./MainControls"
import { TeamControls } from "./TeamControls"
import { Stage, TeamSide } from "@/types";
import { ClocksContainer } from "./ClocksContainer";
import { TimeoutTypePanel } from "./TimeoutTypePanel";
import { useGameStageState } from "@/hooks";
import { useUserSettings } from "@/hooks/UserSettings";

type ControlPanelProps = {
    gameId?: string;
}

export const ControlPanel = ({ gameId }: ControlPanelProps) => {

    const { stage, periodIsFinalized } = useGameStageState() ?? { stage: Stage.BeforeGame, periodIsFinalized: false };

    const userSettings = useUserSettings();

    const teamControlsDisabled = stage === Stage.BeforeGame || stage === Stage.AfterGame && periodIsFinalized;

    return (
        <>
            { userSettings.showClockControls && <MainControls gameId={gameId} /> }
            { userSettings.showClockControls && <TimeoutTypePanel gameId={gameId} /> }
            { (userSettings.showLineupControls || userSettings.showScoreControls || userSettings.showStatsControls) && (
                <div className="w-full flex">
                    <TeamControls side={TeamSide.Home} gameId={gameId} disabled={teamControlsDisabled} />
                    <TeamControls side={TeamSide.Away} gameId={gameId} disabled={teamControlsDisabled} />
                </div>
            )}
            { userSettings.showClocks && <ClocksContainer /> }
        </>
    )
}