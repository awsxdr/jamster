import { IntermissionClockState } from "@/types/IntermissionClockState";
import { Clock, ClockProps } from "./Clock";

type IntermissionClockProps = 
    Omit<ClockProps<IntermissionClockState>, "secondsMapper" | "stateName" | "direction" | "startValue">
    & {
        showTextOnZero?: boolean;
    }

export const IntermissionClock = ({ showTextOnZero, ...props }: IntermissionClockProps) => (
    <Clock<IntermissionClockState> 
        secondsMapper={s => s.secondsRemaining} 
        stateName="IntermissionClockState" 
        direction="up"
        textOnZero={showTextOnZero ? "Starting soon..." : undefined}
        {...props} 
    />
);
