import { PeriodClock, TimeoutClock } from "@/components/clocks"
import { ScoreboardComponent } from "./ScoreboardComponent"
import { GameStageState, TimeoutType } from "@/types"
import { useCurrentTimeoutTypeState } from "@/hooks";
import { useMemo } from "react";
import { ClocksBar } from "./ClocksBar";

type TimeoutDetailsProps = {
    gameStage: GameStageState;
    visible: boolean
}

export const TimeoutDetails = ({ gameStage, visible }: TimeoutDetailsProps) => {

    const { type } = useCurrentTimeoutTypeState() ?? { type: TimeoutType };
    
    const timeoutTypeName = useMemo(() =>
        type === TimeoutType.Official ? 'Official timeout'
        : type === TimeoutType.Review ? 'Official review'
        : type === TimeoutType.Team ? 'Team timeout'
        : 'Timeout'
    , [type]);
    
    return (
        <ClocksBar visible={visible} className="gap-5 flex-col">
            <div className="h-[40%] flex">
            </div>
            <div className="h-[60%] flex gap-5">
                <ScoreboardComponent className="w-1/2 h-full" header={`Period ${gameStage.periodNumber} | Jam ${gameStage.jamNumber}`}>
                    <PeriodClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" autoScale />
                </ScoreboardComponent>
                <ScoreboardComponent className="w-1/2 h-full" header={timeoutTypeName} headerClassName="bg-red-300">
                    <TimeoutClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" autoScale />
                </ScoreboardComponent>
            </div>
        </ClocksBar>
    );
}