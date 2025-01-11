import { PeriodClock, TimeoutClock } from "@/components/clocks"
import { ScoreboardComponent } from "./ScoreboardComponent"
import { GameStageState, TimeoutType } from "@/types"
import { useCurrentTimeoutTypeState, useI18n } from "@/hooks";
import { useMemo } from "react";
import { ClocksBar } from "./ClocksBar";

type TimeoutDetailsProps = {
    gameStage: GameStageState;
    visible: boolean
}

export const TimeoutDetails = ({ gameStage, visible }: TimeoutDetailsProps) => {

    const { type } = useCurrentTimeoutTypeState() ?? { type: TimeoutType };

    const { translate, language } = useI18n();
    
    const timeoutTypeName = useMemo(() =>
        type === TimeoutType.Official ? translate("Scoreboard.TimeoutDetails.OfficialTimeout")
        : type === TimeoutType.Review ? translate("Scoreboard.TimeoutDetails.OfficialReview")
        : type === TimeoutType.Team ? translate("Scoreboard.TimeoutDetails.TeamTimeout")
        : translate("Scoreboard.TimeoutDetails.Timeout")
    , [type, language]);
    
    return (
        <ClocksBar visible={visible} className="gap-1 md:gap-2 lg:gap-5 flex-col">
            <div className="h-[40%] flex">
            </div>
            <div className="h-[60%] flex gap-1 md:gap-2 lg:gap-5">
                <ScoreboardComponent className="w-1/2 h-full" header={`${translate("Scoreboard.TimeoutDetails.Period")} ${gameStage.periodNumber} | ${translate("Scoreboard.TimeoutDetails.Jam")} ${gameStage.jamNumber}`}>
                    <PeriodClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" autoScale />
                </ScoreboardComponent>
                <ScoreboardComponent className="w-1/2 h-full" header={timeoutTypeName} headerClassName="bg-red-300">
                    <TimeoutClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" autoScale />
                </ScoreboardComponent>
            </div>
        </ClocksBar>
    );
}