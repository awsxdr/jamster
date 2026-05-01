import { EventWithoutBody } from "./Event";

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