import { ScoreboardComponent } from "./ScoreboardComponent"
import { GameStageState, Stage } from "@/types"
import { ClocksBar } from "./ClocksBar";
import { IntermissionClock } from "@/components/clocks/IntermissionClock";
import { ScaledText } from "@/components/ScaledText";

const BeforeGameBar = ({ visible }: IntermissionDetailsProps) => (
    <ClocksBar visible={visible} className="gap-5 flex-col">
        <div className="h-[40%] flex">
        </div>
        <div className="h-[60%] flex gap-5">
            <ScoreboardComponent className="w-full h-full pb-5" header="Time to derby">
                <IntermissionClock 
                    showTextOnZero={true}
                    textClassName="flex justify-center items-center h-full m-2 overflow-hidden" 
                    autoScale
                />
            </ScoreboardComponent>
        </div>
    </ClocksBar>
);

const IntermissionBar = ({ visible }: IntermissionDetailsProps) => (
    <ClocksBar visible={visible} className="gap-5 flex-col">
        <div className="h-[40%] flex">
        </div>
        <div className="h-[60%] flex gap-5">
            <ScoreboardComponent className="w-full h-full" header="Intermission">
                <IntermissionClock 
                    showTextOnZero={true}
                    textClassName="flex justify-center items-center h-full m-2 overflow-hidden" 
                    autoScale
                />
            </ScoreboardComponent>
        </div>
    </ClocksBar>
);

const AfterGameBar = ({ gameStage, visible }: IntermissionDetailsProps) => (
    <ClocksBar visible={visible} className="gap-5 flex-col">
        <div className="h-[40%] flex">
        </div>
        <div className="h-[60%] flex gap-5">
            <ScoreboardComponent className="w-full h-full">
                <ScaledText 
                    text={gameStage.periodIsFinalized ? "Final score" : "Unofficial score"} 
                    className="h-full w-full overflow-hidden flex justify-center items-center"
                />
            </ScoreboardComponent>
        </div>
    </ClocksBar>
);

type IntermissionDetailsProps = {
    gameStage: GameStageState;
    visible: boolean;
}

export const IntermissionDetails = (props: IntermissionDetailsProps) => {
    
    switch(props.gameStage.stage) {
        case Stage.BeforeGame:
            return (<BeforeGameBar {...props} />);

        case Stage.AfterGame:
            return (<AfterGameBar {...props} />);

        default:
            return (<IntermissionBar {...props} />);
    }

}