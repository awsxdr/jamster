import { LineupClock, PeriodClock } from "@/components/clocks"
import { ScoreboardComponent } from "./ScoreboardComponent"
import { GameStageState } from "@/types"
import { ClocksBar } from "./ClocksBar";

type LineupDetailsProps = {
    gameStage: GameStageState;
    visible: boolean;
}

export const LineupDetails = ({ gameStage, visible }: LineupDetailsProps) => {
    return (
        <ClocksBar visible={visible} className="gap-5">
            <ScoreboardComponent className="w-1/2 h-full" header={`Period ${gameStage.periodNumber}`}>
                <PeriodClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" />
            </ScoreboardComponent>
            <ScoreboardComponent className="w-1/2 h-full" header={`Lineup (Jam ${gameStage.jamNumber + 1})`}>
                <LineupClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" />
            </ScoreboardComponent>
        </ClocksBar>
    );
}