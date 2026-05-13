import { EventWithBody } from "@/hooks/EventsApiHook";
import { TeamSide } from "../TeamSide"

export class SkaterOnTrack extends EventWithBody {
    constructor(teamSide: TeamSide, position: SkaterPosition, skaterId: string) {
        super("SkaterOnTrack", { teamSide, skaterId, position });
    }
}

export class SkaterOffTrack extends EventWithBody {
    constructor(teamSide: TeamSide, skaterId: string) {
        super("SkaterOffTrack", { teamSide, skaterId });
    }
}

export class SkaterAddedToJam extends EventWithBody {
    constructor(teamSide: TeamSide, period: number, jam: number, position: SkaterPosition, skaterId: string) {
        super("SkaterAddedToJam", { teamSide, period, jam, position, skaterId });
    }
}

export class SkaterRemovedFromJam extends EventWithBody {
    constructor(teamSide: TeamSide, period: number, jam: number, skaterId: string) {
        super("SkaterRemovedFromJam", { teamSide, period, jam, skaterId });
    }
}

export class SkaterInjuryAdded extends EventWithBody {
    constructor(teamSide: TeamSide, skaterId: string) {
        super("SkaterInjuryAdded", { teamSide, skaterId });
    }
}

export class SkaterInjuryRemoved extends EventWithBody {
    constructor(teamSide: TeamSide, skaterId: string, totalJamNumberStart: number) {
        super("SkaterInjuryRemoved", { teamSide, skaterId, totalJamNumberStart });
    }
}

export enum SkaterPosition {
    Jammer = 'Jammer',
    Pivot = 'Pivot',
    Blocker = "Blocker",
}