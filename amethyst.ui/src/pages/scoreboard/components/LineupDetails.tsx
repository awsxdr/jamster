import { LineupClock, PeriodClock } from "@/components/clocks"
import { ScoreboardComponent } from "./ScoreboardComponent"
import { GameStageState } from "@/types"
import { ClocksBar } from "./ClocksBar";
import { useI18n, useLineupClockState, usePeriodClockState } from "@/hooks";
import { SCOREBOARD_GAP_CLASS_NAME } from "../Scoreboard";
import { cn } from "@/lib/utils";
import { useMemo } from "react";

type LineupDetailsProps = {
    gameStage: GameStageState;
    visible: boolean;
}

export const LineupDetails = ({ gameStage, visible }: LineupDetailsProps) => {

    const { translate } = useI18n();

    const periodClock = usePeriodClockState();
    const lineupClock = useLineupClockState();

    const jamWillStart = useMemo(
        () => lineupClock && periodClock && (30 * 60 - periodClock.secondsPassed) - (30 - lineupClock.secondsPassed) > 0,
        [periodClock?.secondsPassed, lineupClock?.secondsPassed]
    );

    return (
        <ClocksBar visible={visible} className="gap-1 md:gap-2 lg:gap-5 flex-col" bottomPanel={
            <div className={cn("flex w-full h-full", SCOREBOARD_GAP_CLASS_NAME)}>
                <ScoreboardComponent className="w-1/2 h-full" header={`${translate("Scoreboard.LineupDetails.Period")} ${gameStage.periodNumber}`}>
                    <PeriodClock 
                        textClassName="flex justify-center items-center grow overflow-hidden" 
                        autoScale 
                    />
                </ScoreboardComponent>
                <ScoreboardComponent className="w-1/2 h-full" headerClassName={!jamWillStart ? "bg-red-300" : ""} header={`${translate("Scoreboard.LineupDetails.Lineup")} (${translate("Scoreboard.LineupDetails.Jam")} ${gameStage.jamNumber + 1})`}>
                    <LineupClock 
                        textClassName="flex justify-center items-center grow overflow-hidden" 
                        autoScale 
                    />
                </ScoreboardComponent>
            </div>
        } />
    );
}