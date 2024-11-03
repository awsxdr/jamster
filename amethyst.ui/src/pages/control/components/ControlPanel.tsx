import { MainControls } from "./MainControls"
import { TeamControls } from "./TeamControls"
import { useGameStageState } from "@/hooks";
import { TeamSide } from "@/types";
import { JamClock, LineupClock, PeriodClock, TimeoutClock } from "@/components/clocks";
import { Clock } from "./Clock";

export const ControlPanel = () => {
    const gameStage = useGameStageState();
    
    return (
        <>
            <MainControls />
            <div className="w-full flex">
                <TeamControls side={TeamSide.Home} />
                <TeamControls side={TeamSide.Away} />
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