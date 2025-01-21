import { useEvents, useTimeoutListState } from "@/hooks";
import { cn } from "@/lib/utils";
import { DisplaySide, TeamSide } from "@/types"
import { TeamReviewLost, TeamReviewRetained } from "@/types/events";
import { TeamTimeoutList } from "./TeamTimeoutList";
import { CombinedTimeoutList } from "./CombinedTimeoutList";

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