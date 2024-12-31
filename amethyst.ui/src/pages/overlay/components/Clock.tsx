import { useClocks, useCurrentTimeoutTypeState, useGameStageState, useTeamDetailsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { Stage, TeamSide, TimeoutType } from "@/types";
import { CSSProperties, useMemo } from "react";

export const Clock = () => {

    const { stage, ...game } = useGameStageState() ?? { stage: Stage.BeforeGame, periodNumber: 0, jamNumber: 0 };
    const { type: timeoutType, side: timeoutSide } = useCurrentTimeoutTypeState() ?? { };

    const { team: { color: homeTimeoutColors } } = useTeamDetailsState(TeamSide.Home) ?? { team: { } };
    const { team: { color: awayTimeoutColors } } = useTeamDetailsState(TeamSide.Away) ?? { team: { } };

    const timeoutColors = useMemo(() => timeoutSide === TeamSide.Home ? homeTimeoutColors : awayTimeoutColors, [timeoutSide]);
    
    const { jamClock, periodClock, lineupClock, timeoutClock } = useClocks() ?? { };

    const Header = () => {

        const getTimeoutText = () =>
            timeoutType === TimeoutType.Official ? "Official timeout"
            : timeoutType === TimeoutType.Review ? "Official review"
            : timeoutType === TimeoutType.Team ? "Team timeout"
            : "Timeout";
        
        const text = useMemo(() =>
            stage === Stage.BeforeGame ? "Upcoming"
            : stage === Stage.Lineup ? "Line-up"
            : stage === Stage.Jam ? "Jam"
            : stage === Stage.Timeout ? getTimeoutText()
            : stage === Stage.AfterTimeout ? "Timeout"
            : ""
        , [stage]);

        const [backgroundColor, textColor] =
            stage === Stage.Timeout && (timeoutType === TimeoutType.Team || timeoutType === TimeoutType.Review)
            ? [timeoutColors?.shirtColor ?? '#000', timeoutColors?.complementaryColor ?? '#fff']
            : ['#000', '#fff'];

        return (
            <div
                style={{'--clock-header-background': backgroundColor, '--clock-header-text-color': textColor} as CSSProperties}
                className="flex items-center justify-center w-full h-[--clock-header-height] [font-size:var(--clock-header-text-size)] leading-[--clock-header-text-size] overflow-hidden bg-[--clock-header-background] text-[--clock-header-text-color] rounded-t-lg text-nowrap"
            >
                {text}
            </div>
        );
    }

    const clockValue = 
        stage === Stage.Jam ? 120 - (jamClock?.secondsPassed ?? 0)
        : stage === Stage.Timeout || stage === Stage.AfterTimeout ? timeoutClock?.secondsPassed ?? 0
        : stage === Stage.Lineup ? lineupClock?.secondsPassed ?? 0
        : 0;
    
    const formatClock = (seconds: number) =>
        seconds >= 60 ? `${Math.floor(seconds / 60)}:${(seconds % 60).toString().padStart(2, '0')}`
        : seconds.toString();

    const clockContainerClassName = "absolute flex flex-col left-[--clock-left] top-[--clock-top] w-[--clock-width] h-[--clock-height] bg-red-500 rounded-lg bg-gradient-to-b from-[#eee] to-[#aaa]";
    const mainClockClassName = "flex justify-center items-center w-full h-[--main-clock-height] [font-size:var(--main-clock-text-size)] leading-[--main-clock-text-size]";
    const periodClockClassName = "flex justify-center items-center w-full h-[--period-clock-height] [font-size:var(--period-clock-text-size)] leading-[--period-clock-text-size]";
    const footerClassName = "flex w-full border-t-black h-[--clock-footer-height] [font-size:var(--clock-footer-text-size)] leading-[--clock-footer-text-size]";
    const footerItemClassName = "flex w-[calc(var(--clock-width)_/_2)] h-full items-center justify-center border-t-[1px] border-[#666]";
    const periodNumberClassName = cn(footerItemClassName, "border-r-[1px]");
    const jamNumberClassName = cn(footerItemClassName, "");

    if (![Stage.Intermission, Stage.Lineup, Stage.Jam, Stage.Timeout, Stage.AfterTimeout].includes(stage)) {
        return (
            <></>
        );
    } 

    return (
        <div className={clockContainerClassName}>
            <Header />
            <div className={mainClockClassName}>
                {formatClock(clockValue)}
            </div>
            <div className={periodClockClassName}>
                {formatClock(30 * 60 - (periodClock?.secondsPassed ?? 0))}
            </div>
            <div className={footerClassName}>
                <div className={periodNumberClassName}>Period {game?.periodNumber ?? 0}</div>
                <div className={jamNumberClassName}>Jam {game?.jamNumber ?? 0}</div>
            </div>
        </div>
    );
}