import { EventWithoutBody } from "@/hooks";

export class IntermissionStarted extends EventWithoutBody {
    constructor() {
        super("IntermissionStarted");
    }
}