import { useI18n, useJamStatsState, useTeamDetailsState, useTeamScoreState, useTeamTimeoutsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { ReviewStatus, TeamSide, TimeoutInUse } from "@/types";
import { switchex } from "@/utilities/switchex";
import { Star, StarOff } from "lucide-react";
import { CSSProperties, useMemo } from "react";

type ScoreRowProps = {
    side: TeamSide;
}

export const ScoreRow = ({ side }: ScoreRowProps) => {

    const sharedScoreRowClassName = "absolute flex gap-[0.3vw] items-center h-[--score-row-height] w-[--score-row-width] left-[--score-row-left] [font-size:var(--score-row-text-size)] leading-[--score-row-text-size] overflow-hidden";
    const homeScoreRowClassName = cn(sharedScoreRowClassName, "top-[--score-row-top] rounded-t-lg border-b-[1px] border-[#888] bg-gradient-to-b from-[#eee] to-[#ccc]");
    const awayScoreRowClassName = cn(sharedScoreRowClassName, "top-[calc(var(--score-row-top)_+_var(--score-row-height))] rounded-b-lg border-t-[1px] border-white bg-gradient-to-b from-[#ccc] to-[#aaa]");

    const timeoutItemClassName = "rounded-full h-[calc(var(--score-row-height)_/_4_-_2px)] w-[calc(var(--score-row-height)_/_4_-_2px)]";

    const { team } = useTeamDetailsState(side) ?? { };
    const { score } = useTeamScoreState(side) ?? { };
    const timeouts = useTeamTimeoutsState(side);
    const { lead, lost } = useJamStatsState(side) ?? { };

    const { translate } = useI18n();

    const teamName = useMemo(() => 
        team?.names['overlay'] 
        ?? team?.names['team'] 
        ?? team?.names['league'] 
        ?? team?.names['color'] 
        ?? translate(side === TeamSide.Home ? "Overlay.ScoreRow.Home" : "Overlay.ScoreRow.Away")
    , [team]);

    const timeoutActive = timeouts?.currentTimeout === TimeoutInUse.Timeout;

    const style = {
        '--team-color': team?.color.shirtColor,
        '--complementary-color': team?.color.complementaryColor,
    } as CSSProperties;

    const reviewClass = cn(
        timeoutItemClassName, 
        "bg-black rounded-none", 
        timeouts?.currentTimeout === TimeoutInUse.Review && "animate-pulse-full-fast",
        timeouts?.reviewStatus === ReviewStatus.Retained && "bg-[#666]"
    );

    return (
        <div className={side === TeamSide.Home ? homeScoreRowClassName : awayScoreRowClassName} style={style}>
            <div className="w-[--score-row-height] flex items-center justify-center">
                { 
                    lead && lost 
                        ? <StarOff className="h-[--star-height]" /> 
                        : lead 
                            ? <Star className="h-[--star-height]" /> 
                            : <></> 
                }
            </div>
            <div className="grow">{teamName}</div>
            <div className="h-full flex flex-col justify-between py-[2px]">
                { Array.from(new Array(3 - (timeouts?.numberTaken ?? 0))).map((_, i) => (
                    <div key={i} className={cn(timeoutItemClassName, "bg-black")}></div>
                ))}
                { timeoutActive && <div className={cn(timeoutItemClassName, "bg-black animate-pulse-full-fast")}></div> }
                { Array.from(new Array((timeouts?.numberTaken ?? 0) - (timeoutActive ? 1 : 0))).map((_, i) => (
                    <div key={i} className={timeoutItemClassName}></div>
                ))}
                {
                    switchex(timeouts?.reviewStatus)
                        .case(ReviewStatus.Unused).then(<div className={reviewClass}></div>)
                        .case(ReviewStatus.Retained).then(<div className={reviewClass}></div>)
                        .default(<div className={timeoutItemClassName}></div>)
                }
            </div>
            <div className="flex w-[calc(var(--score-row-height)_*_2)] h-full items-center justify-center font-bold text-[--complementary-color] bg-[--team-color]">
                {score}
            </div>
        </div>
    );
}