import { Card, CardContent, CardHeader, Separator, Switch } from "@/components/ui";
import { useEvents, useI18n, useTimeoutListState } from "@/hooks";
import { cn } from "@/lib/utils";
import { TeamSide, TimeoutListItem, TimeoutType } from "@/types"
import { TeamReviewLost, TeamReviewRetained } from "@/types/events";
import { useMemo } from "react";

type TimeoutRowProps = {
    sheetSide?: TeamSide;
    timeout: TimeoutListItem;
    onTimeoutRetentionChanged?: (retained: boolean) => void;
}

const TimeoutRow = ({ sheetSide, timeout, onTimeoutRetentionChanged }: TimeoutRowProps) => {

    const timeoutName = useMemo(() => 
        timeout.type === TimeoutType.Official ? "Official time out"
        : timeout.type === TimeoutType.Team && timeout.side === sheetSide ? "Timeout this team"
        : timeout.type === TimeoutType.Team ? "Timeout other team"
        : timeout.type === TimeoutType.Review && timeout.side === sheetSide ? "Official review this team"
        : timeout.type === TimeoutType.Review ? "Official review other team"
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
                { timeout.type === TimeoutType.Review && timeout.side === sheetSide && (
                    <>Retained <Switch checked={timeout.retained} onCheckedChange={onTimeoutRetentionChanged} /></>
                )}
            </span>
        </div>
    );
}

type TimeoutListProps = {
    side: TeamSide;
    gameId?: string;
    className?: string;
}

export const TimeoutList = ({ side, gameId, className }: TimeoutListProps) => {
    const { timeouts } = useTimeoutListState() ?? { timeouts: [] };

    const sortedTimeouts = useMemo(() => [...timeouts].sort((a, b) => b.eventId.localeCompare(a.eventId)), [timeouts]);

    const { sendEvent } = useEvents();

    const { translate } = useI18n();

    if(!gameId) {
        return <></>
    }

    const handleTimeoutRetentionChanged = (eventId: string, retained: boolean) => {
        sendEvent(gameId, retained ? new TeamReviewRetained(side, eventId) : new TeamReviewLost(side, eventId));
    }

    return (
        <Card className={cn("w-full", className)}>
            <CardHeader className="font-bold text-center">
                { side === TeamSide.Home ? translate("TimeoutList.HomeTitle") : translate("TimeoutList.AwayTitle")}
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
                        sheetSide={side} 
                        onTimeoutRetentionChanged={retained => handleTimeoutRetentionChanged(timeout.eventId, retained)} 
                    />
                )}
            </CardContent>
        </Card>
    )
}