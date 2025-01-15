import { useTeamTimeoutsState } from "@/hooks";
import { useMemo } from "react";
import { ScoreboardComponent } from "./ScoreboardComponent";
import { ScaledText } from '@components/ScaledText';
import { ReviewStatus, TeamSide, TimeoutInUse } from "@/types";
import { cn } from "@/lib/utils";
import { SCOREBOARD_GAP_CLASS_NAME } from "../Scoreboard";

type TeamTimeoutsProps = {
    side: TeamSide
};

enum TimeoutSymbolState {
    Default,
    Retained,
    Hidden,
}

type TimeoutSymbolProps = {
    state: TimeoutSymbolState,
    active?: boolean,
};

const TimeoutSymbol = ({ state, active }: TimeoutSymbolProps) => {
    const symbol = useMemo(() => 
        state === TimeoutSymbolState.Retained ? "✚" 
        : state === TimeoutSymbolState.Hidden ? ""
        : "⬤", 
    [state]);

    return (
        <ScaledText className={cn("flex grow shrink-0 basis-[0] text-black justify-center items-center w-full", active ? "animate-pulse-full-fast" : "")} text={symbol} />
    );
}

export const TeamTimeouts = ({ side }: TeamTimeoutsProps) => {

    const timeouts = useTeamTimeoutsState(side);

    const reviewSymbolState = useMemo(() => {
        switch(timeouts?.reviewStatus ?? ReviewStatus.Unused) {
            case ReviewStatus.Unused: return TimeoutSymbolState.Default;
            case ReviewStatus.Retained: return TimeoutSymbolState.Retained;
            default: return TimeoutSymbolState.Hidden;
        };
    }, [timeouts]);

    const timeoutActive = useMemo(() => timeouts?.currentTimeout === TimeoutInUse.Timeout, [timeouts]);

    return (
        <div className={cn("flex flex-col grow h-full mh-full", SCOREBOARD_GAP_CLASS_NAME)}>
            <ScoreboardComponent className="flex flex-col py-1 md:py-2 items-center text-center grow-[3]">
                { Array.from(new Array(timeouts?.numberRemaining ?? 3)).map((_, i) => (<TimeoutSymbol key={i} state={TimeoutSymbolState.Default} />))}
                { timeoutActive && <TimeoutSymbol state={TimeoutSymbolState.Default} active /> }
                { Array.from(new Array((timeoutActive ? 2 : 3) - (timeouts?.numberRemaining ?? 3))).map((_, i) => (<TimeoutSymbol key={3 - i} state={TimeoutSymbolState.Hidden} />))}
            </ScoreboardComponent>
            <ScoreboardComponent className="flex flex-col py-2 items-center text-center grow">
                <TimeoutSymbol state={reviewSymbolState} active={timeouts?.currentTimeout === TimeoutInUse.Review} />
            </ScoreboardComponent>
        </div>
    );
}