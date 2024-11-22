import { MainControls } from "./MainControls"
import { TeamControls } from "./TeamControls"
import { TeamSide } from "@/types";
import { ClocksContainer } from "./ClocksContainer";
import { TimeoutTypePanel } from "./TimeoutTypePanel";

type ControlPanelProps = {
    gameId?: string;
}

export const ControlPanel = ({ gameId }: ControlPanelProps) => {
    return (
        <>
            <MainControls gameId={gameId} />
            <TimeoutTypePanel gameId={gameId} />
            <div className="w-full flex">
                <TeamControls side={TeamSide.Home} gameId={gameId} />
                <TeamControls side={TeamSide.Away} gameId={gameId} />
            </div>
            <ClocksContainer />
        </>
    )
}