import { useGameState, useRulesState } from "@/hooks";
import { Clock, ClockProps } from "./Clock";
import { PeriodClockState } from "@/types";

type PeriodClockProps = Omit<ClockProps, "seconds" | "isRunning" | "direction" | "startValue">;

export const PeriodClock = (props: PeriodClockProps) => {

    const clockState = useGameState<PeriodClockState>("PeriodClockState");

    const { rules } = useRulesState() ?? { };

    if(!rules) {
        return (<></>);
    }

    return (
        <Clock
            seconds={clockState?.secondsPassed} 
            isRunning={clockState?.isRunning ?? false}
            direction="down" 
            startValue={rules.periodRules.durationInSeconds}
            {...props} />
    );
}