import { useGameState } from "@/hooks";
import { useMemo } from "react";
import { ScaledText } from "@components/ScaledText";
import { cn } from "@/lib/utils";

export type ClockProps<TClockState> = {
    secondsMapper: (state: TClockState) => number;
    stateName: string;
    direction: "down" | "up";
    textOnZero?: string;
    startValue?: number;
    textClassName?: string;
};

export const Clock = <TClockState,>({ secondsMapper, stateName, direction, textOnZero, startValue, textClassName }: ClockProps<TClockState>) => {
    const clockState = useGameState<TClockState>(stateName);
    
    const clock = useMemo(() => clockState && secondsMapper(clockState), [secondsMapper, clockState]);

    const time = useMemo(() => {
        if(clock === undefined) {
            return textOnZero ?? '0';
        }

        const totalSeconds = direction === 'up' ? clock : ((startValue ?? 0) - clock);

        if (totalSeconds === 0 && textOnZero) {
            return textOnZero;
        }

        const minutes = Math.floor(totalSeconds / 60);
        const seconds = totalSeconds % 60;

        return minutes > 0 ? `${minutes}:${seconds.toString().padStart(2, '0')}` : `${seconds}`;
    }, [clock, direction, startValue]);

    return (
        <>
            <ScaledText text={time} className={cn("w-full", textClassName)} />
        </>
    );
};
