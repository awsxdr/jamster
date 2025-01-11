import { Card, CardContent, CardHeader, Separator, Switch } from "@/components/ui";
import { useI18n } from "@/hooks";
import { cn } from "@/lib/utils";
import { TeamSide, TimeoutListItem, TimeoutType } from "@/types"
import { useMemo } from "react";

type TimeoutRowProps = {
    timeout: TimeoutListItem;
    onTimeoutRetentionChanged?: (side: TeamSide, retained: boolean) => void;
}

const TimeoutRow = ({ timeout, onTimeoutRetentionChanged }: TimeoutRowProps) => {

    const timeoutName = useMemo(() => 
        timeout.type === TimeoutType.Official ? "Official time out"
        : timeout.type === TimeoutType.Team && timeout.side === TeamSide.Home ? "Home team timeout"
        : timeout.type === TimeoutType.Team ? "Away team timeout"
        : timeout.type === TimeoutType.Review && timeout.side === TeamSide.Home ? "Home team official review"
        : timeout.type === TimeoutType.Review ? "Away team official review"
        : "Untyped"
    , [timeout]);

    const formatTime = (totalSeconds: number) => {
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = totalSeconds % 60;

        return `${minutes}:${seconds.toString().padStart(2, '0')}`;
    }

    return (
        <div className="w-full p-2 flex flex-nowrap items-center">
            <span className="w-1/5">P1 J2</span>
            <span className="w-2/5">{ timeoutName }</span>
            <span className="w-1/5 text-center">{ timeout.durationInSeconds ? formatTime(timeout.durationInSeconds) : 'In progress' }</span>
            <span className="flex flex-wrap gap-2 w-1/5 justify-end">
                { timeout.type === TimeoutType.Review && (
                    <>Retained <Switch checked={timeout.retained} onCheckedChange={v => onTimeoutRetentionChanged?.(timeout.side!, v)} /></>
                )}
            </span>
        </div>
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

    const { translate } = useI18n();

    if(!gameId) {
        return <></>
    }

    return (
        <Card className={cn("w-full", className)}>
            <CardHeader className="font-bold text-center">
                { translate("CombinedTimeoutList.Title") }
            </CardHeader>
            <CardContent>
                <div className="flex flex-nowrap p-2">
                    <span className="font-bold w-1/5"></span>
                    <span className="font-bold w-2/5">Type</span>
                    <span className="font-bold w-1/5 text-center">Duration</span>
                </div>
                <Separator />
                {sortedTimeouts.map(timeout => 
                    <TimeoutRow
                        key={timeout.eventId}
                        timeout={timeout} 
                        onTimeoutRetentionChanged={(side, retained) => onRetentionChanged?.(side, timeout.eventId, retained)} 
                    />
                )}
            </CardContent>
        </Card>
    )
}