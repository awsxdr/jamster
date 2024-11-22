import { API_URL } from "@/constants";

type EventsApi = {
    sendEvent: (gameId: string, event: Event) => Promise<void>;
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

export const useEvents: () => EventsApi = () => {
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

    return {
        sendEvent
    }
}