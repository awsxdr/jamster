import { useGameState } from "@/hooks";
import { TeamSide } from "./TeamScore";
import { useMemo } from "react";
import { ScoreboardComponent } from "./ScoreboardComponent";
import styles from './TeamTimeouts.module.scss';
import { ScaledText } from "./ScaledText";

type TeamTimeoutsProps = {
    side: TeamSide
};

type TeamTimeoutsState = {
    numberRemaining: number,
    reviewStatus: ReviewStatus,
    currentTimeout: TimeoutInUse,
};

enum TimeoutInUse {
    None,
    Timeout,
    Review
}

enum ReviewStatus {
    Unused,
    Retained,
    Used,
}

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

    const gameState = useGameState();
    const timeouts = gameState.useStateWatch<TeamTimeoutsState>(`TeamTimeoutsState_${TeamSide[side]}`);

    const reviewSymbolState = useMemo(() =>
        timeouts?.reviewStatus === ReviewStatus.Unused ? TimeoutSymbolState.Default
        : timeouts?.reviewStatus === ReviewStatus.Retained ? TimeoutSymbolState.Retained
        : TimeoutSymbolState.Hidden,
    [timeouts]);

    return (
        <>
            <ScoreboardComponent className={styles.container}>
                { Array.from(new Array(timeouts?.numberRemaining ?? 3)).map(() => (<TimeoutSymbol state={TimeoutSymbolState.Default} />))}
                { timeouts?.currentTimeout === TimeoutInUse.Timeout && <TimeoutSymbol state={TimeoutSymbolState.Default} active /> }
                { Array.from(new Array(3 - (timeouts?.numberRemaining ?? 3))).map(() => (<TimeoutSymbol state={TimeoutSymbolState.Hidden} />))}
            </ScoreboardComponent>
            <ScoreboardComponent className={styles.reviewContainer}>
                <TimeoutSymbol state={reviewSymbolState} active={timeouts?.currentTimeout === TimeoutInUse.Review} />
            </ScoreboardComponent>
        </>
    );
}