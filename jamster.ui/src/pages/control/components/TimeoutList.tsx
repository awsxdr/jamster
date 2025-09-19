import { useEvents, useI18n, useTimeoutListState } from "@/hooks";
import { cn } from "@/lib/utils";
import { DisplaySide, TeamSide } from "@/types"
import { TeamReviewLost, TeamReviewRetained } from "@/types/events";
import { TeamTimeoutList } from "./TeamTimeoutList";
import { CombinedTimeoutList } from "./CombinedTimeoutList";
import { useState } from "react";
import { Button, Card, CardContent, CardHeader, Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui";
import { ChevronRight } from "lucide-react";

type TimeoutListProps = {
    gameId?: string;
    displaySide: DisplaySide;
    className?: string;
}

export const TimeoutList = ({ gameId, displaySide, className }: TimeoutListProps) => {

    const { timeouts } = useTimeoutListState() ?? { timeouts: [] };
    const { sendEvent } = useEvents();
    const { translate } = useI18n();
    const [open, setIsOpen] = useState(true);

    if(!gameId) {
        return <></>
    }

    const handleTimeoutRetentionChanged = (side: TeamSide, eventId: string, retained: boolean) => {
        sendEvent(gameId, retained ? new TeamReviewRetained(side, eventId) : new TeamReviewLost(side, eventId));
    }

    return (
        <Collapsible open={open} onOpenChange={setIsOpen}>
            <Card className={cn("w-full", className)}>
                <CardHeader className="flex flex-row relative">
                    <div className="grow text-xl">
                        { translate("CombinedTimeoutList.Title") }
                    </div>
                    <CollapsibleTrigger asChild>
                        <Button className="transition group/collapsible absolute top-2 right-2" variant="ghost" size="icon">
                            <ChevronRight className="transition duration-300 ease-in-out group-data-[state=open]/collapsible:rotate-90" />
                        </Button>
                    </CollapsibleTrigger>
                </CardHeader>
                <CollapsibleContent>
                    <CardContent className="w-full">
                        <div className="hidden 2xl:grid grid-flow-rows gap-2 auto-cols-fr">
                            { displaySide !== DisplaySide.Away && 
                                <TeamTimeoutList 
                                    side={TeamSide.Home} 
                                    gameId={gameId} 
                                    timeouts={timeouts}
                                    className="col-start-1"
                                    onRetentionChanged={handleTimeoutRetentionChanged}
                                />
                            }
                            { displaySide !== DisplaySide.Home && 
                                <TeamTimeoutList 
                                    side={TeamSide.Away} 
                                    gameId={gameId} 
                                    timeouts={timeouts}
                                    className="col-start-2"
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
                    </CardContent>
                </CollapsibleContent>
            </Card>
        </Collapsible>    
    )
}