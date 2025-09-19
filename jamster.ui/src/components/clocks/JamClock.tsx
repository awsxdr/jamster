import { JamClockState } from "@/types";
import { Clock, ClockProps } from "./Clock";
import { useJamClockState } from "@/hooks";

type JamClockProps = Omit<ClockProps<JamClockState>, "secondsMapper" | "stateName" | "direction" | "startValue">;

export const JamClock = (props: JamClockProps) => {

    const clockState = useJamClockState();

    return (
        <Clock
            secondsMapper={s => s.secondsPassed} 
            state={clockState}
            direction="down" 
            startValue={120} 
            {...props} 
        />
    );
}
