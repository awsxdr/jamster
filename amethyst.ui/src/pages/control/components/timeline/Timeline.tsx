import { ScrollArea, ScrollBar } from "@/components/ui"
import { useEvents, useTimelineState } from "@/hooks"
import { Fragment, useEffect, useRef } from "react";
import { TimelineItem, TimelineSeparator } from ".";

type TimelineProps = {
    gameId: string;
}

export const Timeline = ({ gameId }: TimelineProps) => {

    const timeline = useTimelineState();
    const { moveEvent } = useEvents();

    const lastItemRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        if(lastItemRef.current === null) {
            return;
        }

        lastItemRef.current.scrollIntoView();
    }, [lastItemRef.current]);
    
    if(!timeline) {
        return <></>;
    }

    const handleAdjust = (eventId: string, tick: number) => {
        moveEvent(gameId, eventId, tick, true);
    }

    return (
        <div className="bg-white text-white h-[100px] w-full">
            <ScrollArea>
                <div className="flex h-[100px] justify-stretch p-2">
                    {
                        timeline.previousStages.slice(1).map((stage, i) => (
                            <Fragment key={stage.eventId}>
                                { i > 0 && (
                                    <TimelineSeparator onAdjust={s => handleAdjust(stage.eventId, stage.startTick + s * 1000)} />
                                )}
                                <TimelineItem key={stage.eventId} stage={stage.stage} duration={stage.duration} scale={1.0} />
                            </Fragment>
                        ))
                    }
                    <TimelineSeparator onAdjust={s => handleAdjust(timeline.currentStageEventId, timeline.currentStageStartTick + s * 1000)} />
                    <TimelineItem stage={timeline.currentStage} duration={0} scale={1.0} final ref={lastItemRef} />
                </div>
                <ScrollBar orientation="horizontal" />
            </ScrollArea>
        </div>
    )
}