import { Clock, ClockProps } from "./Clock";

type IntermissionClockState = {
    isRunning: boolean;
    hasExpired: boolean;
    targetTick: number;
    secondsRemaining: number;
};

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
