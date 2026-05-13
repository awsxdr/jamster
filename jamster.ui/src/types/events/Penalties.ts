import { EventWithBody } from "@/hooks";
import { TeamSide } from "@/types";

export class SkaterSatInBox extends EventWithBody {
    constructor(teamSide: TeamSide, skaterId: string) {
        super("SkaterSatInBox", { teamSide, skaterId });
    }
}

export class SkaterReleasedFromBox extends EventWithBody {
    constructor(teamSide: TeamSide, skaterId: string) {
        super("SkaterReleasedFromBox", { teamSide, skaterId });
    }
}

export class SkaterSubstitutedInBox extends EventWithBody {
    constructor(teamSide: TeamSide, originalSkaterId: string, newSkaterId: string) {
        super("SkaterSubstitutedInBox", { teamSide, originalSkaterId, newSkaterId });
    }
}

export class PenaltyAssessed extends EventWithBody {
    constructor(teamSide: TeamSide, skaterId: string, penaltyCode: string) {
        super("PenaltyAssessed", { teamSide, skaterId, penaltyCode });
    }
}

export class PenaltyRescinded extends EventWithBody {
    constructor(teamSide: TeamSide, skaterId: string, penaltyCode: string, period: number, jam: number) {
        super("PenaltyRescinded", { teamSide, skaterId, penaltyCode, period, jam });
    }
}

export class PenaltyUpdated extends EventWithBody {
    constructor(teamSide: TeamSide, skaterId: string, originalPenaltyCode: string, originalPeriod: number, originalJam: number, newPenaltyCode: string, newPeriod: number, newJam: number) {
        super("PenaltyUpdated", {
            teamSide,
            skaterId,
            originalPenaltyCode,
            originalPeriod,
            originalJam,
            newPenaltyCode,
            newPeriod,
            newJam
        });
    }
}

export class SkaterExpelled extends EventWithBody {
    constructor(teamSide: TeamSide, skaterId: string, penaltyCode: string, period: number, jam: number) {
        super("SkaterExpelled", { teamSide, skaterId, penaltyCode, period, jam });
    }
}

export class ExpulsionCleared extends EventWithBody {
    constructor(teamSide: TeamSide, skaterId: string) {
        super("ExpulsionCleared", { teamSide, skaterId });
    }
}

export class PenaltyServedSet extends EventWithBody {
    constructor(teamSide: TeamSide, skaterId: string, penaltyCode: string, period: number, jam: number, served: boolean) {
        super("PenaltyServedSet", { teamSide, skaterId, penaltyCode, period, jam, served });
    }
}