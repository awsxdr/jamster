import { CSSProperties, useMemo } from "react";
import { ScoreRow } from "./components/ScoreRow";
import { TeamSide } from "@/types";
import { GameStateContextProvider, useCurrentGame } from "@/hooks";
import { useSearchParams } from "react-router-dom";
import { Clock } from "./components/Clock";

export const Overlay = () => {

    const [ searchParams ] = useSearchParams();
    const { currentGame } = useCurrentGame();

    const gameId = useMemo(() => searchParams.get('gameId') || currentGame?.id, [searchParams, currentGame]);

    const showBackground = false;

    const scale = 1.0;

    const style = {
        '--score-row-width': `${25 * scale}vw`,
        '--score-row-height': `${3 * scale}vh`,
        '--score-row-top': `${2 * scale}vh`,
        '--score-row-left': `${2 * scale}vw`,
        '--score-row-text-size': `${1.5 * scale}vh`,
        '--clock-top': `${2 * scale}vh`,
        '--clock-left': `${100 - 12 * scale}vw`,
        '--clock-width': `${10 * scale}vw`,
        '--clock-height': `${10 * scale}vh`,
        '--clock-header-text-size': `${1.5 * scale}vh`,
        '--clock-header-height': `${2 * scale}vh`,
        '--main-clock-height': `${3 * scale}vh`,
        '--main-clock-text-size': `${2.5 * scale}vh`,
        '--period-clock-height': `${2.5 * scale}vh`,
        '--period-clock-text-size': `${1.75 * scale}vh`,
        '--clock-footer-height': `${2.5 * scale}vh`,
        '--clock-footer-text-size': `${1.25 * scale}vh`,
    } as CSSProperties;

    return (
        <GameStateContextProvider gameId={gameId}>
            { showBackground && (
                <div className="absolute left-0 top-0 right-0 bottom-0 bg-[#0f0]">
                </div>
            )}
            <div className="h-0 w-0 relative" style={style}>
                <ScoreRow side={TeamSide.Home} />
                <ScoreRow side={TeamSide.Away} />
                <Clock />
            </div>
        </GameStateContextProvider>
    )
}