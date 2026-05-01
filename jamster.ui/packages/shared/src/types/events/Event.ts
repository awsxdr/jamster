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