import { LineupClock, PeriodClock } from "@/components/clocks"
import { ScoreboardComponent } from "./ScoreboardComponent"
import { GameStageState } from "@/types"
import { ClocksBar } from "./ClocksBar";
import { useI18n } from "@/hooks";

type LineupDetailsProps = {
    gameStage: GameStageState;
    visible: boolean;
}

export const LineupDetails = ({ gameStage, visible }: LineupDetailsProps) => {

    const { translate } = useI18n();

    return (
        <ClocksBar visible={visible} className="gap-1 md:gap-2 lg:gap-5 flex-col">
            <div className="h-[40%] flex">
            </div>
            <div className="h-[60%] flex gap-1 md:gap-2 lg:gap-5">
                <ScoreboardComponent className="w-1/2 h-full" header={`${translate("Scoreboard.LineupDetails.Period")} ${gameStage.periodNumber}`}>
                    <PeriodClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" autoScale />
                </ScoreboardComponent>
                <ScoreboardComponent className="w-1/2 h-full" header={`${translate("Scoreboard.LineupDetails.Lineup")} (${translate("Scoreboard.LineupDetails.Jam")} ${gameStage.jamNumber + 1})`}>
                    <LineupClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" autoScale />
                </ScoreboardComponent>
            </div>
        </ClocksBar>
    );
}