import { EventWithBody } from "@/hooks";
import { TeamSide } from "@/types";

export class SkaterSatInBox extends EventWithBody {
    constructor(teamSide: TeamSide, skaterNumber: string) {
        super("SkaterSatInBox", { teamSide, skaterNumber });
    }
}

export class SkaterReleasedFromBox extends EventWithBody {
    constructor(teamSide: TeamSide, skaterNumber: string) {
        super("SkaterReleasedFromBox", { teamSide, skaterNumber });
    }
}
