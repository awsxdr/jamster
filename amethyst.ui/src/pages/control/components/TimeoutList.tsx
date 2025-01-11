import { Card, CardContent, CardHeader, Separator, Switch } from "@/components/ui";
import { DisplaySide, useEvents, useI18n, useTimeoutListState } from "@/hooks";
import { cn } from "@/lib/utils";
import { TeamSide, TimeoutListItem, TimeoutType } from "@/types"
import { TeamReviewLost, TeamReviewRetained } from "@/types/events";
import { useMemo } from "react";
import { TeamTimeoutList } from "./TeamTimeoutList";
import { CombinedTimeoutList } from "./CombinedTimeoutList";

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
    gameId?: string;
    displaySide: DisplaySide;
    className?: string;
}

export const TimeoutList = ({ gameId, displaySide, className }: TimeoutListProps) => {
    const { timeouts } = useTimeoutListState() ?? { timeouts: [] };

    const { sendEvent } = useEvents();

    if(!gameId) {
        return <></>
    }

    const handleTimeoutRetentionChanged = (side: TeamSide, eventId: string, retained: boolean) => {
        sendEvent(gameId, retained ? new TeamReviewRetained(side, eventId) : new TeamReviewLost(side, eventId));
    }

    return (
        <>
            <div className={cn("w-full hidden flex-nowrap gap-2 2xl:flex", className)}>
                { displaySide !== DisplaySide.Away && 
                    <TeamTimeoutList 
                        side={TeamSide.Home} 
                        gameId={gameId} 
                        timeouts={timeouts}
                        className={displaySide == DisplaySide.Both ? "xl:w-1/2" : ""} 
                        onRetentionChanged={handleTimeoutRetentionChanged}
                    />
                }
                { displaySide !== DisplaySide.Home && 
                    <TeamTimeoutList 
                        side={TeamSide.Away} 
                        gameId={gameId} 
                        timeouts={timeouts}
                        className={displaySide == DisplaySide.Both ? "xl:w-1/2" : ""} 
                        onRetentionChanged={handleTimeoutRetentionChanged}
                    />
                }
            </div>
            <div className={cn("w-full flex 2xl:hidden", className)}>
                <CombinedTimeoutList
                    gameId={gameId}
                    timeouts={timeouts}
                    onRetentionChanged={handleTimeoutRetentionChanged}
                />
            </div>
        </>
    )
}