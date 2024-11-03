import { MainControls } from "./MainControls"
import { TeamControls } from "./TeamControls"
import { useGameStageState } from "@/hooks";
import { TeamSide } from "@/types";
import { JamClock, LineupClock, PeriodClock, TimeoutClock } from "@/components/clocks";
import { Clock } from "./Clock";

type ControlPanelProps = {
    gameId?: string;
}

export const ControlPanel = ({ gameId }: ControlPanelProps) => {
    const gameStage = useGameStageState();
    
    return (
        <>
            <MainControls gameId={gameId} />
            <div className="w-full flex">
                <TeamControls side={TeamSide.Home} gameId={gameId} />
                <TeamControls side={TeamSide.Away} gameId={gameId} />
            </div>
            <div className="w-full flex space-x-2">
                <Clock name={`Jam ${gameStage?.jamNumber ?? 0}`} clock={c => <JamClock textClassName={c} />} />
                <Clock name={`Period ${gameStage?.periodNumber ?? 0}`} clock={c => <PeriodClock textClassName={c} />} />
                <Clock name="Lineup" clock={c => <LineupClock textClassName={c} />} />
                <Clock name="Timeout" clock={c => <TimeoutClock textClassName={c} />} />
                <Clock name="Intermission" clock={c => <JamClock textClassName={c} />} />
            </div>
        </>
    )
}