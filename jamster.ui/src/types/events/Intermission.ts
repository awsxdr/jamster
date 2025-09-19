import { EventWithoutBody } from "@/hooks";

export class IntermissionStarted extends EventWithoutBody {
    constructor() {
        super("IntermissionStarted");
    }
}

export class IntermissionEnded extends EventWithoutBody {
    constructor() {
        super("IntermissionEnded");
    }
}