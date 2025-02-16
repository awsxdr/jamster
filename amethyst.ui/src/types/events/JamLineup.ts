import { EventWithBody } from "@/hooks/EventsApiHook";
import { TeamSide } from "../TeamSide"

export class SkaterOnTrack extends EventWithBody {
    constructor(teamSide: TeamSide, position: SkaterPosition, skaterNumber: string) {
        super("SkaterOnTrack", { teamSide, skaterNumber, position });
    }
}

export class SkaterOffTrack extends EventWithBody {
    constructor(teamSide: TeamSide, skaterNumber: string) {
        super("SkaterOffTrack", { teamSide, skaterNumber });
    }
}

export class SkaterAddedToJam extends EventWithBody {
    constructor(teamSide: TeamSide, period: number, jam: number, position: SkaterPosition, skaterNumber: string) {
        super("SkaterAddedToJam", { teamSide, period, jam, position, skaterNumber });
    }
}

export class SkaterRemovedFromJam extends EventWithBody {
    constructor(teamSide: TeamSide, period: number, jam: number, skaterNumber: string) {
        super("SkaterRemovedFromJam", { teamSide, period, jam, skaterNumber });
    }
}

export enum SkaterPosition {
    Jammer = 'Jammer',
    Pivot = 'Pivot',
    Blocker = "Blocker",
}