import { JamClock, PeriodClock } from "@/components/clocks"
import { ScoreboardComponent } from "./ScoreboardComponent"
import { GameStageState } from "@/types"
import { ClocksBar } from "./ClocksBar";

type JamDetailsProps = {
    gameStage: GameStageState;
    visible: boolean;
}

export const JamDetails = ({ gameStage, visible }: JamDetailsProps) => {
    return (
        <ClocksBar visible={visible} className="gap-5">
            <ScoreboardComponent className="w-1/2 h-full" header={`Period ${gameStage.periodNumber}`}>
                <PeriodClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" />
            </ScoreboardComponent>
            <ScoreboardComponent className="w-1/2 h-full" header={`Jam ${gameStage.jamNumber}`}>
                <JamClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" />
            </ScoreboardComponent>
        </ClocksBar>
    );
}