import { Separator, Switch } from "@/components/ui";
import { cn } from "@/lib/utils";
import { TeamSide, TimeoutListItem, TimeoutType } from "@/types"
import { switchex } from "@/utilities/switchex";
import { useMemo } from "react";

type TimeoutRowProps = {
    timeout: TimeoutListItem;
    onTimeoutRetentionChanged?: (side: TeamSide, retained: boolean) => void;
}

const TimeoutRow = ({ timeout, onTimeoutRetentionChanged }: TimeoutRowProps) => {

    const timeoutName = useMemo(() => 
        switchex(timeout.type)
            .case(TimeoutType.Official).then("Official time out")
            .case(TimeoutType.Team).when(() => timeout.side === TeamSide.Home).then("Home team timeout")
            .case(TimeoutType.Team).then("Away team timeout")
            .case(TimeoutType.Review).when(() => timeout.side === TeamSide.Home).then("Home team official review")
            .case(TimeoutType.Review).then("Away team official review")
            .default("Untyped")
    , [timeout]);

    const formatTime = (totalSeconds: number) => {
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = totalSeconds % 60;

        return `${minutes}:${seconds.toString().padStart(2, '0')}`;
    }
    

    return (
        <>
            <span className="col-start-1">{ `P-${timeout.period} J-${timeout.jam}` }</span>
            <span className="col-start-2">{ timeoutName }</span>
            <span className="col-start-3 text-center">{ timeout.durationInSeconds ? formatTime(timeout.durationInSeconds) : 'In progress' }</span>
            <span className="col-start-4">
                { timeout.type === TimeoutType.Review && (
                    <>Retained <Switch checked={timeout.retained} onCheckedChange={v => onTimeoutRetentionChanged?.(timeout.side ?? TeamSide.Home , v)} /></>
                )}
            </span>
        </>
    );
}

type CombinedTimeoutListProps = {
    timeouts: TimeoutListItem[];
    gameId?: string;
    className?: string;
    onRetentionChanged?: (side: TeamSide, eventId: string, retained: boolean) => void;
}

export const CombinedTimeoutList = ({ timeouts, gameId, className, onRetentionChanged }: CombinedTimeoutListProps) => {
    const sortedTimeouts = useMemo(() => [...timeouts].sort((a, b) => b.eventId.localeCompare(a.eventId)), [timeouts]);

    if(!gameId) {
        return <></>
    }

    return (
        <div className={cn("grid grid-flow-rows grid-cols-[1fr_2fr_1fr_1fr] w-full gap-1", className)}>
            <span className="col-start-1 font-bold"></span>
            <span className="col-start-2 font-bold">Type</span>
            <span className="col-start-3 font-bold text-center">Duration</span>
            <Separator className="col-start-1 col-span-4" />
            {sortedTimeouts.map(timeout => 
                <TimeoutRow
                    key={timeout.eventId}
                    timeout={timeout} 
                    onTimeoutRetentionChanged={(side, retained) => onRetentionChanged?.(side, timeout.eventId, retained)} 
                />
            )}
        </div>
    )
}