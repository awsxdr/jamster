import { PeriodClock, TimeoutClock } from "@/components/clocks"
import { ScoreboardComponent } from "./ScoreboardComponent"
import { GameStageState, TimeoutType } from "@/types"
import { useCurrentTimeoutTypeState, useI18n } from "@/hooks";
import { useMemo } from "react";
import { ClocksBar } from "./ClocksBar";
import { cn } from "@/lib/utils";
import { SCOREBOARD_GAP_CLASS_NAME } from "../Scoreboard";

type TimeoutDetailsProps = {
    gameStage: GameStageState;
    visible: boolean
}

export const TimeoutDetails = ({ gameStage, visible }: TimeoutDetailsProps) => {

    const { type } = useCurrentTimeoutTypeState() ?? { type: TimeoutType };

    const { translate, language } = useI18n();
    
    const timeoutTypeName = useMemo(() => (() => {
        if (type === TimeoutType.Official) return translate("Scoreboard.TimeoutDetails.OfficialTimeout");
        else if (type === TimeoutType.Review) return translate("Scoreboard.TimeoutDetails.OfficialReview");
        else if (type === TimeoutType.Team) return translate("Scoreboard.TimeoutDetails.TeamTimeout");
        else return translate("Scoreboard.TimeoutDetails.Timeout");
    })()
    , [type, language]);
    
    return (
        <ClocksBar visible={visible} className={cn("flex-col", SCOREBOARD_GAP_CLASS_NAME)} bottomPanel={
            <div className={cn("flex w-full h-full", SCOREBOARD_GAP_CLASS_NAME)}>
                <ScoreboardComponent className="w-1/2 h-full" header={`${translate("Scoreboard.TimeoutDetails.Period")} ${gameStage.periodNumber} | ${translate("Scoreboard.TimeoutDetails.Jam")} ${gameStage.jamNumber}`}>
                    <PeriodClock 
                        textClassName="flex justify-center items-center grow overflow-hidden leading-none" 
                        autoScale={1.4}
                    />
                </ScoreboardComponent>
                <ScoreboardComponent className="w-1/2 h-full" header={timeoutTypeName} headerClassName="bg-red-300">
                    <TimeoutClock 
                        textClassName="flex justify-center items-center grow overflow-hidden leading-none" 
                        autoScale={1.4}
                    />
                </ScoreboardComponent>
            </div>
        } />
    );
}