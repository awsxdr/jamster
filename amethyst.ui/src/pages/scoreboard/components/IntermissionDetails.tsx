import { ScoreboardComponent } from "./ScoreboardComponent"
import { GameStageState, Stage } from "@/types"
import { ClocksBar } from "./ClocksBar";
import { IntermissionClock } from "@/components/clocks/IntermissionClock";
import { ScaledText } from "@/components/ScaledText";

const BeforeGameBar = ({ visible }: IntermissionDetailsProps) => (
    <ClocksBar visible={visible}>
        <span>Starting soon</span>
    </ClocksBar>
);

const IntermissionBar = ({ visible }: IntermissionDetailsProps) => (
        <ClocksBar visible={visible}>
            <ScoreboardComponent className="w-full h-full" header="Intermission">
                <IntermissionClock 
                    showTextOnZero={true}
                    textClassName="flex justify-center items-center h-full m-2 overflow-hidden" 
                />
            </ScoreboardComponent>
        </ClocksBar>
    );

const AfterGameBar = ({ gameStage, visible }: IntermissionDetailsProps) => (
    <ClocksBar visible={visible}>
        <ScoreboardComponent>
            <ScaledText text={gameStage.periodIsFinalized ? "Final score" : "Unofficial score"} />
        </ScoreboardComponent>
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