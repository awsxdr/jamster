import { ScoreboardComponent } from "./ScoreboardComponent"
import { GameStageState, Stage } from "@/types"
import { ClocksBar } from "./ClocksBar";
import { IntermissionClock } from "@/components/clocks/IntermissionClock";
import { ScaledText } from "@/components/ScaledText";
import { useI18n } from "@/hooks";
import { cn } from "@/lib/utils";
import { SCOREBOARD_GAP_CLASS_NAME } from "../Scoreboard";

const BeforeGameBar = ({ visible }: IntermissionDetailsProps) => {
    const { translate } = useI18n();
    
    return (
        <ClocksBar visible={visible} className={cn("flex-col", SCOREBOARD_GAP_CLASS_NAME)} bottomPanel={
            <ScoreboardComponent className="w-full h-full" header={translate("Scoreboard.IntermissionDetails.TimeToDerby")}>
                <IntermissionClock 
                    showTextOnZero={true}
                    textClassName="flex justify-center items-center grow overflow-hidden leading-none" 
                    autoScale={1.4}
                />
            </ScoreboardComponent>
        }/>
    );
}

const IntermissionBar = ({ visible }: IntermissionDetailsProps) => {
    const { translate } = useI18n();

    return  (
        <ClocksBar visible={visible} className={cn("flex-col", SCOREBOARD_GAP_CLASS_NAME)} bottomPanel={
            <ScoreboardComponent className="w-full h-full" header={translate("Scoreboard.IntermissionDetails.Intermission")}>
                <IntermissionClock 
                    showTextOnZero={true}
                    textClassName="flex justify-center items-center grow overflow-hidden leading-none" 
                    autoScale={1.4}
                />
            </ScoreboardComponent>
        } />
    );
}

const AfterGameBar = ({ gameStage, visible }: IntermissionDetailsProps) => {
    const { translate } = useI18n();
    
    return (
        <ClocksBar visible={visible} className={cn("flex-col", SCOREBOARD_GAP_CLASS_NAME)} bottomPanel={
            <ScoreboardComponent className="w-full h-full">
                <ScaledText 
                    text={gameStage.periodIsFinalized ? translate("Scoreboard.IntermissionDetails.FinalScore") : translate("Scoreboard.IntermissionDetails.UnofficialScore")} 
                    className="flex justify-center items-center grow overflow-hidden leading-none"
                    scale={1.4}
                />
            </ScoreboardComponent>
        } />
    );
}

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