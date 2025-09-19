import { useGameState, useRulesState } from "@/hooks";
import { Clock, ClockProps } from "./Clock";
import { JamClockState } from "@/types";

type JamClockProps = Omit<ClockProps, "seconds" | "isRunning" | "direction" | "startValue">;

export const JamClock = (props: JamClockProps) => {

    const clockState = useGameState<JamClockState>("JamClockState");

    const { rules } = useRulesState() ?? { };

    if(!rules) {
        return (<></>);
    }

    return (
        <Clock
            seconds={clockState?.secondsPassed} 
            isRunning={clockState?.isRunning ?? false}
            direction="down" 
            startValue={rules.jamRules.durationInSeconds} 
            {...props} 
        />
    );
}