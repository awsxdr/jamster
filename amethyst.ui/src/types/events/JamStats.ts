import { EventWithBody } from "@/hooks/EventsApiHook";
import { TeamSide } from "..";

export class LeadMarked extends EventWithBody {
    constructor(side: TeamSide, lead: boolean) {
        super("LeadMarked", { side, lead });
    }
}

export class LostMarked extends EventWithBody {
    constructor(side: TeamSide, lost: boolean) {
        super("LostMarked", { side, lost });
    }
}

export class CallMarked extends EventWithBody {
    constructor(side: TeamSide, call: boolean) {
        super("CallMarked", { side, call });
    }
}

export class StarPassMarked extends EventWithBody {
    constructor(side: TeamSide, starPass: boolean) {
        super("StarPassMarked", { side, starPass });
    }
}

export class InitialTripCompleted extends EventWithBody {
    constructor(side: TeamSide, tripCompleted: boolean) {
        super("InitialTripCompleted", { side, tripCompleted });
    }
}