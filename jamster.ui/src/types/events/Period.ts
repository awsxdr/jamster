import { EventWithoutBody } from "@/hooks/EventsApi";

export class PeriodFinalized extends EventWithoutBody {
    constructor() {
        super("PeriodFinalized");
    }
}