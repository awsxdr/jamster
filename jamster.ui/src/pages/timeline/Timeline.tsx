import { EventStreamEvent, SortOrder, eventsApi } from "@/hooks"
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { EventList } from "./components/EventList";

export const Timeline = () => {
    const [ events, setEvents ] = useState<EventStreamEvent[]>([]);
    const [ listDirty, setListDirty ] = useState(true);
    
    const { gameId } = useParams();

    useEffect(() => {
        if(!gameId || !listDirty) {
            return;
        }

        (async () => {
            const events = await eventsApi.getEvents(gameId, { order: SortOrder.Desc });
            setEvents(events);
        })();

        setListDirty(false);
    }, [gameId, listDirty]);

    const handleEventMoved = (eventId: string, newTick: number) => {
        if(!gameId) {
            return;
        }

        eventsApi.moveEvent(gameId, eventId, newTick, true).then(() => setListDirty(true));
    }

    const handleEventDeleted = (eventId: string) => {
        if(!gameId) { 
            return;
        }

        eventsApi.deleteEvent(gameId, eventId).then(() => setListDirty(true));
    }

    return (
        <>
            <EventList events={events} onEventMoved={handleEventMoved} onEventDeleted={handleEventDeleted} />
        </>
    )
}