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

export class SkaterSubstitutedInBox extends EventWithBody {
    constructor(teamSide: TeamSide, originalSkaterNumber: string, newSkaterNumber: string) {
        super("SkaterSubstitutedInBox", { teamSide, originalSkaterNumber, newSkaterNumber });
    }
}

export class PenaltyAssessed extends EventWithBody {
    constructor(teamSide: TeamSide, skaterNumber: string, penaltyCode: string) {
        super("PenaltyAssessed", { teamSide, skaterNumber, penaltyCode });
    }
}

export class PenaltyRescinded extends EventWithBody {
    constructor(teamSide: TeamSide, skaterNumber: string, penaltyCode: string, period: number, jam: number) {
        super("PenaltyRescinded", { teamSide, skaterNumber, penaltyCode, period, jam });
    }
}

export class PenaltyUpdated extends EventWithBody {
    constructor(teamSide: TeamSide, skaterNumber: string, originalPenaltyCode: string, originalPeriod: number, originalJam: number, newPenaltyCode: string, newPeriod: number, newJam: number) {
        super("PenaltyUpdated", {
            teamSide,
            skaterNumber,
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
    constructor(teamSide: TeamSide, skaterNumber: string, penaltyCode: string, period: number, jam: number) {
        super("SkaterExpelled", { teamSide, skaterNumber, penaltyCode, period, jam });
    }
}

export class ExpulsionCleared extends EventWithBody {
    constructor(teamSide: TeamSide, skaterNumber: string) {
        super("ExpulsionCleared", { teamSide, skaterNumber });
    }
}
