import { useTeamTimeoutsState } from "@/hooks";
import { useMemo } from "react";
import { ScoreboardComponent } from "./ScoreboardComponent";
import { ScaledText } from '@components/ScaledText';
import { ReviewStatus, TeamSide, TimeoutInUse } from "@/types";

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

const TimeoutSymbol = ({ state }: TimeoutSymbolProps) => {
    const symbol = useMemo(() => 
        state === TimeoutSymbolState.Retained ? "✚" 
        : state === TimeoutSymbolState.Hidden ? ""
        : "⬤", 
    [state]);

    return (
        <ScaledText className="flex grow shrink-0 basis-[0] text-black justify-center items-center w-full" text={symbol} />
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

    return (
        <div className="flex flex-col grow h-full mh-full">
            <ScoreboardComponent className="flex flex-col m-5 mt-0 py-2 items-center text-center grow-[3]">
                { Array.from(new Array(timeouts?.numberRemaining ?? 3)).map((_, i) => (<TimeoutSymbol key={i} state={TimeoutSymbolState.Default} />))}
                { timeouts?.currentTimeout === TimeoutInUse.Timeout && <TimeoutSymbol state={TimeoutSymbolState.Default} active /> }
                { Array.from(new Array(3 - (timeouts?.numberRemaining ?? 3))).map((_, i) => (<TimeoutSymbol key={3 - i} state={TimeoutSymbolState.Hidden} />))}
            </ScoreboardComponent>
            <ScoreboardComponent className="flex flex-col mx-5 my-0 py-2 items-center text-center grow">
                <TimeoutSymbol state={reviewSymbolState} active={timeouts?.currentTimeout === TimeoutInUse.Review} />
            </ScoreboardComponent>
        </div>
    );
}