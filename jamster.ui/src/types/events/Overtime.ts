import { EventWithoutBody } from "@/hooks";

export class OvertimeStarted extends EventWithoutBody {
    constructor() {
        super("OvertimeStarted");
    }
}

export class OvertimeEnded extends EventWithoutBody {
    constructor() {
        super("OvertimeEnded");
    }
}