import { EventWithoutBody } from "@/hooks/EventsApiHook";

export class JamStarted extends EventWithoutBody {
    constructor() {
        super("JamStarted");
    }
}

export class JamEnded extends EventWithoutBody {
    constructor() {
        super("JamEnded");
    }
}