import { useMemo } from "react";
import { ScaledText } from "@components/ScaledText";
import { cn } from "@/lib/utils";

export type ClockProps<TClockState> = {
    secondsMapper: (state: TClockState) => number;
    state?: TClockState;
    direction: "down" | "up";
    textOnZero?: string;
    startValue?: number;
    textClassName?: string;
    autoScale?: boolean;
};

export const Clock = <TClockState,>({ secondsMapper, state, direction, textOnZero, startValue, textClassName, autoScale }: ClockProps<TClockState>) => {
    const clock = useMemo(() => state && secondsMapper(state), [secondsMapper, state]);

    const time = useMemo(() => {
        if(clock === undefined) {
            return textOnZero ?? '0';
        }

        const totalSeconds = direction === 'up' ? clock : Math.max(0, (startValue ?? 0) - clock);

        if (totalSeconds === 0 && textOnZero) {
            return textOnZero;
        }

        const minutes = Math.floor(totalSeconds / 60);
        const seconds = totalSeconds % 60;

        return minutes > 0 ? `${minutes}:${seconds.toString().padStart(2, '0')}` : `${seconds}`;
    }, [clock, direction, startValue, textOnZero]);

    return (
        <>
        {
            autoScale
            ? <ScaledText text={time} className={cn("w-full", textClassName)} />
            : <span className={textClassName}>{time}</span>
        }
        </>
    );
};
