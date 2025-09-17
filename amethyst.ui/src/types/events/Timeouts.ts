import { EventWithBody, EventWithoutBody } from "@/hooks/EventsApiHook";
import { TeamSide } from "../TeamSide";

export class TimeoutTypeSet extends EventWithBody {
    constructor(body: TimeoutTypeSetBody) {
        super("TimeoutTypeSet", body);
    }
}

export type TimeoutTypeSetBody = {
    type: "Untyped" | "Team" | "Review" | "Official";
    teamSide?: TeamSide;
}

export class TimeoutStarted extends EventWithoutBody {
    constructor() {
        super("TimeoutStarted");
    }
}

export class TimeoutEnded extends EventWithoutBody {
    constructor() {
        super("TimeoutEnded");
    }
}

export class TeamReviewRetained extends EventWithBody {
    constructor(teamSide: TeamSide, timeoutEventId: string) {
        super("TeamReviewRetained", { teamSide, timeoutEventId });
    }
}

export class TeamReviewLost extends EventWithBody {
    constructor(teamSide: TeamSide, timeoutEventId: string) {
        super("TeamReviewLost", { teamSide, timeoutEventId });
    }
}