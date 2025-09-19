import { EventWithoutBody } from "@/hooks/EventsApiHook";

export class PeriodFinalized extends EventWithoutBody {
    constructor() {
        super("PeriodFinalized");
    }
}