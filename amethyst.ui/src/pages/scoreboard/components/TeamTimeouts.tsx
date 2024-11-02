import { useGameState } from "@/hooks";
import { useMemo } from "react";
import { ScoreboardComponent } from "./ScoreboardComponent";
import styles from './TeamTimeouts.module.scss';
import { ScaledText } from '@components/ScaledText';
import { ReviewStatus, TeamSide, TeamTimeoutsState, TimeoutInUse } from "@/types";

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
        <ScaledText className={styles.symbol} text={symbol} />
    );
}

export const TeamTimeouts = ({ side }: TeamTimeoutsProps) => {

    const timeouts = useGameState<TeamTimeoutsState>(`TeamTimeoutsState_${TeamSide[side]}`);

    const reviewSymbolState = useMemo(() => {
        switch(timeouts?.reviewStatus ?? ReviewStatus.Unused) {
            case ReviewStatus.Unused: return TimeoutSymbolState.Default;
            case ReviewStatus.Retained: return TimeoutSymbolState.Retained;
            default: return TimeoutSymbolState.Hidden;
        };
    }, [timeouts]);

    return (
        <div className={styles.container}>
            <ScoreboardComponent className={styles.teamTimeoutsContainer}>
                { Array.from(new Array(timeouts?.numberRemaining ?? 3)).map((_, i) => (<TimeoutSymbol key={i} state={TimeoutSymbolState.Default} />))}
                { timeouts?.currentTimeout === TimeoutInUse.Timeout && <TimeoutSymbol state={TimeoutSymbolState.Default} active /> }
                { Array.from(new Array(3 - (timeouts?.numberRemaining ?? 3))).map((_, i) => (<TimeoutSymbol key={3 - i} state={TimeoutSymbolState.Hidden} />))}
            </ScoreboardComponent>
            <ScoreboardComponent className={styles.reviewContainer}>
                <TimeoutSymbol state={reviewSymbolState} active={timeouts?.currentTimeout === TimeoutInUse.Review} />
            </ScoreboardComponent>
        </div>
    );
}