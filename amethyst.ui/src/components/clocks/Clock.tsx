import { useGameState } from "@/hooks";
import { useMemo } from "react";
import { ScaledText } from "@components/ScaledText";

type ClockProps<TClockState> = {
    secondsMapper: (state: TClockState) => number,
    stateName: string,
    direction: "down" | "up",
    startValue?: number,
};

export const Clock = <TClockState,>({ secondsMapper, stateName, direction, startValue }: ClockProps<TClockState>) => {
    const gameState = useGameState();
    const clockState = gameState.useStateWatch<TClockState>(stateName);
    
    const clock = useMemo(() => clockState && secondsMapper(clockState), [secondsMapper, clockState]);

    const time = useMemo(() => {
        if(clock === undefined) {
            return '0';
        }

        const totalSeconds = direction === 'up' ? clock : ((startValue ?? 0) - clock);
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = totalSeconds % 60;

        return minutes > 0 ? `${minutes}:${seconds.toString().padStart(2, '0')}` : `${seconds}`;
    }, [clock, direction, startValue]);

    return (
        <>
            <ScaledText text={time} />
        </>
    );
};
