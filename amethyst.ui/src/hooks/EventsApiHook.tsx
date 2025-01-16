import { API_URL } from "@/constants";
import { StringMap } from "@/types";
import * as uuid from 'uuid';

export enum SortOrder {
    Asc = "Asc",
    Desc = "Desc",
}

type EventsApi = {
    getEvents: (gameId: string, options: { skip?: number, take?: number, order?: SortOrder }) => Promise<EventStreamEvent[]>;
    sendEvent: (gameId: string, event: Event) => Promise<void>;
    moveEvent: (gameId: string, eventId: string, tick: number) => Promise<void>;
    deleteEvent: (gameId: string, eventId: string) => Promise<void>;
}

export abstract class Event {
    name: string;
    hasBody: boolean;

    constructor(name: string, hasBody: boolean) {
        this.name = name;
        this.hasBody = hasBody;
    }
}

export abstract class EventWithoutBody extends Event {
    constructor(name: string) {
        super(name, false);
    }
}

export abstract class EventWithBody extends Event {
    body: object;

    constructor(name: string, body: object) {
        super(name, true);
        this.body = body;
    }
}

export type EventStreamEvent = {
    type: string;
    id: string;
    tick: number;
    body: object | null;
}

export const useEvents: () => EventsApi = () => {

    const buildQuery = (values: StringMap<any>) =>
        Object.keys(values).filter(k => values[k]).map(k => `${k}=${values[k]}`).join("&");

    const getEvents = async (gameId: string, options: { skip?: number, take?: number, order?: SortOrder }) => {
        const query = buildQuery({skip: options.skip, maxCount: options.take, sortOrder: options.order});
        const result = await fetch(`${API_URL}/api/Games/${gameId}/events?${query}`);

        const events = await result.json() as Omit<EventStreamEvent, "tick">[];

        const getTick = (id: string) => {
            const parsedUuid = uuid.parse(id);
            const tickBytes = new Int8Array(8);
            tickBytes[0] = parsedUuid[5];
            tickBytes[1] = parsedUuid[4];
            tickBytes[2] = parsedUuid[3];
            tickBytes[3] = parsedUuid[2];
            tickBytes[4] = parsedUuid[1];
            tickBytes[5] = parsedUuid[0];
            const data = new DataView(tickBytes.buffer);
            return parseInt(data.getBigUint64(0, true).toString());
        }
        
        return events.map(e => ({ ...e, tick: getTick(e.id)}));
    }

    const sendEvent = async (gameId: string, event: Event) => {
        await fetch(`${API_URL}/api/Games/${gameId}/events`, {
            method: 'POST',
            body: JSON.stringify({
                type: event.name,
                body: event.hasBody && (event as EventWithBody).body || undefined
            }),
            headers: {
                "Content-type": "application/json; charset=utf-8",
            }
        });
    }

    const moveEvent = async (gameId: string, eventId: string, tick: number) => {
        await fetch(`${API_URL}/api/Games/${gameId}/events/${eventId}/tick`, {
            method: 'PUT',
            body: JSON.stringify({
                tick
            }),
            headers: {
                "Content-type": "application/json; charset=utf-8",
            }
        });
    }

    const deleteEvent = async (gameId: string, eventId: string) => {
        await fetch(`${API_URL}/api/Games/${gameId}/events/${eventId}`, {
            method: 'DELETE',
        });
    }

    return {
        getEvents,
        sendEvent,
        moveEvent,
        deleteEvent,
    }
}