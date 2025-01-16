import { EventStreamEvent, SortOrder, useEvents } from "@/hooks"
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { EventList } from "./components/EventList";

export const Timeline = () => {

    const { getEvents, moveEvent, deleteEvent } = useEvents();
    const [ events, setEvents ] = useState<EventStreamEvent[]>([]);
    const [ listDirty, setListDirty ] = useState(true);
    
    const { gameId } = useParams();

    useEffect(() => {
        if(!gameId || !listDirty) {
            return;
        }

        (async () => {
            const events = await getEvents(gameId, { order: SortOrder.Desc });
            setEvents(events);
        })();

        setListDirty(false);
    }, [gameId, listDirty]);

    const handleEventMoved = (eventId: string, newTick: number) => {
        if(!gameId) {
            return;
        }

        moveEvent(gameId, eventId, newTick).then(() => setListDirty(true));
    }

    const handleEventDeleted = (eventId: string) => {
        if(!gameId) { 
            return;
        }

        deleteEvent(gameId, eventId).then(() => setListDirty(true));
    }

    return (
        <>
            <EventList events={events} onEventMoved={handleEventMoved} onEventDeleted={handleEventDeleted} />
        </>
    )
}