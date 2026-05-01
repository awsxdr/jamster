import { EventWithoutBody } from "./Event";

export class PeriodFinalized extends EventWithoutBody {
    constructor() {
        super("PeriodFinalized");
    }
}