import { IntermissionClockState } from "@/types/IntermissionClockState";
import { Clock, ClockProps } from "./Clock";
import { useI18n } from "@/hooks";

type IntermissionClockProps = 
    Omit<ClockProps<IntermissionClockState>, "secondsMapper" | "stateName" | "direction" | "startValue">
    & {
        showTextOnZero?: boolean;
    }

export const IntermissionClock = ({ showTextOnZero, ...props }: IntermissionClockProps) => {
    const { translate } = useI18n();

    return (
        <Clock<IntermissionClockState> 
            secondsMapper={s => s.secondsRemaining} 
            stateName="IntermissionClockState" 
            direction="up"
            textOnZero={showTextOnZero ? translate("IntermissionClock.StartingSoon") : undefined}
            {...props} 
        />
    );
}