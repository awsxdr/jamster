import { Card, CardContent, CardHeader, Separator, Switch } from "@/components/ui";
import { useI18n } from "@/hooks";
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

type TeamTimeoutListProps = {
    side: TeamSide;
    timeouts: TimeoutListItem[];
    gameId?: string;
    className?: string;
    onRetentionChanged?: (side: TeamSide, eventId: string, retained: boolean) => void;
}

export const TeamTimeoutList = ({ side, timeouts, gameId, className, onRetentionChanged }: TeamTimeoutListProps) => {
    const sortedTimeouts = useMemo(() => [...timeouts].sort((a, b) => b.eventId.localeCompare(a.eventId)), [timeouts]);

    const { translate } = useI18n();

    if(!gameId) {
        return <></>
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
                        onTimeoutRetentionChanged={retained => onRetentionChanged?.(side, timeout.eventId, retained)} 
                    />
                )}
            </CardContent>
        </Card>
    )
}