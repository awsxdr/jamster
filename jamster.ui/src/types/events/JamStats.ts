import { EventWithBody } from "@/hooks/EventsApiHook";
import { TeamSide } from "..";

export class LeadMarked extends EventWithBody {
    constructor(teamSide: TeamSide, lead: boolean) {
        super("LeadMarked", { teamSide, lead });
    }
}

export class LostMarked extends EventWithBody {
    constructor(teamSide: TeamSide, lost: boolean) {
        super("LostMarked", { teamSide, lost });
    }
}

export class CallMarked extends EventWithBody {
    constructor(teamSide: TeamSide, call: boolean) {
        super("CallMarked", { teamSide, call });
    }
}

export class StarPassMarked extends EventWithBody {
    constructor(teamSide: TeamSide, starPass: boolean) {
        super("StarPassMarked", { teamSide, starPass });
    }
}

export class InitialTripCompleted extends EventWithBody {
    constructor(teamSide: TeamSide, tripCompleted: boolean) {
        super("InitialTripCompleted", { teamSide, tripCompleted });
    }
}