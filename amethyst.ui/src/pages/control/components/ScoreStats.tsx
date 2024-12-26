import { Card, CardContent, Separator, Switch } from "@/components/ui";
import { useEvents, useTimeoutListState } from "@/hooks";
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
        <div className="w-full p-2 flex items-center">
            <span className="w-1/3">{ timeoutName }</span>
            <span className="w-1/3 text-center">{ timeout.durationInSeconds ? formatTime(timeout.durationInSeconds) : 'In progress' }</span>
            <span className="flex gap-2 w-1/3 justify-end">
                { timeout.type === TimeoutType.Review && timeout.side === sheetSide && (
                    <>Retained <Switch checked={timeout.retained} onCheckedChange={onTimeoutRetentionChanged} /></>
                )}
            </span>
        </div>
    );
}

type ScoreStatsProps = {
    side: TeamSide;
    gameId?: string;
    className?: string;
}

export const ScoreStats = ({ side, gameId, className }: ScoreStatsProps) => {
    const { timeouts } = useTimeoutListState() ?? { timeouts: [] };

    const { sendEvent } = useEvents();

    if(!gameId) {
        return <></>
    }

    const handleTimeoutRetentionChanged = (eventId: string, retained: boolean) => {
        sendEvent(gameId, retained ? new TeamReviewRetained(side, eventId) : new TeamReviewLost(side, eventId));
    }

    return (
        <Card className={className}>
            <CardContent className="pt-4">
                <div className="flex p-2">
                    <span className="font-bold w-1/3">Type</span>
                    <span className="font-bold w-1/3 text-center">Duration</span>
                </div>
                <Separator />
                {timeouts.map(timeout => 
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