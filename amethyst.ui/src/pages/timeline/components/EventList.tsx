import { EventStreamEvent } from "@/hooks"
import { EventListItem } from "./EventListItem";
import { Card, CardContent } from "@/components/ui";

type EventListProps = {
    events: EventStreamEvent[];
    onEventMoved: (eventId: string, newTick: number) => void;
    onEventDeleted: (eventId: string) => void;
}

export const EventList = ({ events, onEventMoved, onEventDeleted }: EventListProps) => {
    
    const minTime = Math.min(...events.map(e => e.tick));

    return (
        <>
            <Card className="m-4 p-4">
                <CardContent className="p-0 flex flex-col gap-2">
                    {events.map(e => (
                        <EventListItem key={e.id} event={e} minTime={minTime} onEventMoved={onEventMoved} onEventDeleted={onEventDeleted} />
                    ))}
                </CardContent>
            </Card>
        </>
    )
}