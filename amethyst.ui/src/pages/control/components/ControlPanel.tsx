import { MainControls } from "./MainControls"
import { TeamControls } from "./TeamControls"
import { Stage, TeamSide } from "@/types";
import { ClocksContainer } from "./ClocksContainer";
import { TimeoutTypePanel } from "./TimeoutTypePanel";
import { useGameStageState } from "@/hooks";

type ControlPanelProps = {
    gameId?: string;
}

export const ControlPanel = ({ gameId }: ControlPanelProps) => {

    const { stage, periodIsFinalized } = useGameStageState() ?? { stage: Stage.BeforeGame, periodIsFinalized: false };

    return (
        <>
            <MainControls gameId={gameId} />
            <TimeoutTypePanel gameId={gameId} />
            <div className="w-full flex">
                <TeamControls side={TeamSide.Home} gameId={gameId} disabled={stage === Stage.AfterGame && periodIsFinalized} />
                <TeamControls side={TeamSide.Away} gameId={gameId} disabled={stage === Stage.AfterGame && periodIsFinalized} />
            </div>
            <ClocksContainer />
        </>
    )
}