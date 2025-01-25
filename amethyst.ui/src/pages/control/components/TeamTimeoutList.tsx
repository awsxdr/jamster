import { Card, CardContent, Separator, Switch } from "@/components/ui";
import { cn } from "@/lib/utils";
import { TeamSide, TimeoutListItem, TimeoutType } from "@/types"
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
        <>
            <span className="col-start-1 ">{ `P-${timeout.period} J-${timeout.jam}` }</span>
            <span className="col-start-2 ">{ timeoutName }</span>
            <span className="col-start-3 text-center">{ timeout.durationInSeconds ? formatTime(timeout.durationInSeconds) : 'In progress' }</span>
            <span className="col-start-4 flex flex-wrap gap-2 justify-end">
                { timeout.type === TimeoutType.Review && timeout.side === sheetSide && (
                    <>Retained <Switch checked={timeout.retained} onCheckedChange={onTimeoutRetentionChanged} /></>
                )}
            </span>
        </>
    );
}

type TeamTimeoutListProps = {
    side: TeamSide;
    timeouts: TimeoutListItem[];
    gameId?: string;
    className?: string;
    onRetentionChanged?: (side: TeamSide, eventId: string, retained: boolean) => void;
}

export const TeamTimeoutList = ({ side, timeouts, gameId, className, onRetentionChanged }: TeamTimeoutListProps) => {
    const sortedTimeouts = useMemo(() => [...timeouts].sort((a, b) => b.eventId.localeCompare(a.eventId)), [timeouts]);

    if(!gameId) {
        return <></>
    }

    return (
        <Card className={cn("w-full", className)}>
            <CardContent>
                <div className={cn("grid grid-flow-rows grid-cols-[1fr_2fr_1fr_1fr] w-full gap-1", className)}>
                    <span className="font-bold"></span>
                    <span className="font-bold">Type</span>
                    <span className="font-bold text-center">Duration</span>
                    <Separator />
                    {sortedTimeouts.map(timeout => 
                        <TimeoutRow
                            key={timeout.eventId}
                            timeout={timeout} 
                            sheetSide={side} 
                            onTimeoutRetentionChanged={retained => onRetentionChanged?.(side, timeout.eventId, retained)} 
                        />
                    )}
                </div>
            </CardContent>
        </Card>
    )
}