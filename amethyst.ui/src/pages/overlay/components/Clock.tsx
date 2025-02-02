import { useClocks, useCurrentTimeoutTypeState, useGameStageState, useI18n, useTeamDetailsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { Stage, TeamSide, TimeoutType } from "@/types";
import { CSSProperties, useMemo } from "react";

export const Clock = () => {

    const { translate } = useI18n();

    const { stage, ...game } = useGameStageState() ?? { stage: Stage.BeforeGame, periodNumber: 0, jamNumber: 0, periodIsFinalized: false };
    const { type: timeoutType, teamSide: timeoutSide } = useCurrentTimeoutTypeState() ?? { };

    const { team: { color: homeTimeoutColors } } = useTeamDetailsState(TeamSide.Home) ?? { team: { } };
    const { team: { color: awayTimeoutColors } } = useTeamDetailsState(TeamSide.Away) ?? { team: { } };

    const timeoutColors = useMemo(() => timeoutSide === TeamSide.Home ? homeTimeoutColors : awayTimeoutColors, [timeoutSide]);
    
    const { jamClock, periodClock, lineupClock, timeoutClock, intermissionClock } = useClocks() ?? { };

    const Header = () => {

        const getTimeoutText = () =>
            timeoutType === TimeoutType.Official ? translate("Overlay.Clock.OfficialTimeout")
            : timeoutType === TimeoutType.Review ? translate("Overlay.Clock.OfficialReview")
            : timeoutType === TimeoutType.Team ? translate("Overlay.Clock.TeamTimeout")
            : translate("Overlay.Clock.Timeout");
        
        const text = useMemo(() =>
            stage === Stage.BeforeGame ? translate("Overlay.Clock.Upcoming")
            : stage === Stage.Lineup ? translate("Overlay.Clock.Lineup")
            : stage === Stage.Jam ? translate("Overlay.Clock.Jam")
            : stage === Stage.Timeout ? getTimeoutText()
            : stage === Stage.AfterTimeout ? translate("Overlay.Clock.Timeout")
            : stage === Stage.Intermission ? translate("Overlay.Clock.Intermission")
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

    const clockContainerClassName = "absolute flex flex-col left-[--clock-left] top-[--clock-top] w-[--clock-width] h-[--clock-height] rounded-lg bg-gradient-to-b from-[#eee] to-[#aaa]";
    const intermissionClockClassName = "absolute flex flex-col left-[--clock-left] top-[--clock-top] w-[--clock-width] h-[--intermission-clock-height] rounded-lg bg-gradient-to-b from-[#eee] to-[#aaa]";
    const intermissionClockTextClassName = "flex justify-center items-center h-full w-full [font-size:var(--intermission-clock-text-size)] leading-[--intermission-clock-text-size]";
    const postGameClassName = "absolute flex flex-col justify-center items-center left-[--clock-left] top-[--clock-top] w-[--clock-width] h-[--post-game-height] rounded-lg bg-black text-white [font-size:var(--post-game-text-size)] leading-[--post-game-text-size]";
    const mainClockClassName = "flex justify-center items-center w-full h-[--main-clock-height] [font-size:var(--main-clock-text-size)] leading-[--main-clock-text-size]";
    const periodClockClassName = "flex justify-center items-center w-full h-[--period-clock-height] [font-size:var(--period-clock-text-size)] leading-[--period-clock-text-size]";
    const footerClassName = "flex w-full border-t-black h-[--clock-footer-height] [font-size:var(--clock-footer-text-size)] leading-[--clock-footer-text-size]";
    const footerItemClassName = "flex w-[calc(var(--clock-width)_/_2)] h-full items-center justify-center border-t-[1px] border-[#666] text-nowrap overflow-hidden";
    const periodNumberClassName = cn(footerItemClassName, "border-r-[1px]");
    const jamNumberClassName = cn(footerItemClassName, "");

    if (stage === Stage.BeforeGame || stage === Stage.Intermission) {
        return (
            <div className={intermissionClockClassName}>
                <Header />
                <div className={intermissionClockTextClassName}>
                    {!intermissionClock?.hasExpired ? formatClock(intermissionClock?.secondsRemaining ?? 0) : translate("Overlay.Clock.StartingSoon")}
                </div>
            </div>
        );
    }

    if (stage === Stage.AfterGame) {
        return (
            <div className={postGameClassName}>
                {game.periodIsFinalized ? translate("Overlay.Clock.FinalScore") : translate("Overlay.Clock.UnofficialScore")}
            </div>
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
                <div className={periodNumberClassName}>{translate("Overlay.Clock.Period")} {game?.periodNumber ?? 0}</div>
                <div className={jamNumberClassName}>{translate("Overlay.Clock.Jam")} {game?.jamNumber ?? 0}</div>
            </div>
        </div>
    );
}