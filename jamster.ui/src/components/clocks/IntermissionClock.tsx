import { IntermissionClockState } from "@/types/IntermissionClockState";
import { Clock, ClockProps } from "./Clock";
import { useI18n, useIntermissionClockState } from "@/hooks";

type IntermissionClockProps = 
    Omit<ClockProps<IntermissionClockState>, "secondsMapper" | "stateName" | "direction" | "startValue">
    & {
        showTextOnZero?: boolean;
    }

export const IntermissionClock = ({ showTextOnZero, ...props }: IntermissionClockProps) => {
    const { translate } = useI18n();

    const clockState = useIntermissionClockState();

    return (
        <Clock
            secondsMapper={s => s.secondsRemaining} 
            state={clockState}
            direction="up"
            textOnZero={showTextOnZero ? translate("IntermissionClock.StartingSoon") : undefined}
            {...props} 
        />
    );
}