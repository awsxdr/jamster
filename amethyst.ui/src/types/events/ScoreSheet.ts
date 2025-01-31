import { EventWithBody } from "@/hooks";
import { TeamSide } from "../TeamSide";

export class ScoreSheetJammerNumberSet extends EventWithBody {
    constructor(teamSide: TeamSide, lineNumber: number, jammerNumber: string) {
        super("ScoreSheetJammerNumberSet", { teamSide, totalJamNumber: lineNumber, value: jammerNumber });
    }
}

export class ScoreSheetPivotNumberSet extends EventWithBody {
    constructor(teamSide: TeamSide, totalJamNumber: number, pivotNumber: string) {
        super("ScoreSheetPivotNumberSet", { teamSide, totalJamNumber, value: pivotNumber });
    }
}

export class ScoreSheetLostSet extends EventWithBody {
    constructor(teamSide: TeamSide, lineNumber: number, lost: boolean) {
        super("ScoreSheetLostSet", { teamSide, totalJamNumber: lineNumber, value: lost });
    }
}

export class ScoreSheetLeadSet extends EventWithBody {
    constructor(teamSide: TeamSide, lineNumber: number, lost: boolean) {
        super("ScoreSheetLeadSet", { teamSide, totalJamNumber: lineNumber, value: lost });
    }
}

export class ScoreSheetCalledSet extends EventWithBody {
    constructor(teamSide: TeamSide, lineNumber: number, lost: boolean) {
        super("ScoreSheetCalledSet", { teamSide, totalJamNumber: lineNumber, value: lost });
    }
}

export class ScoreSheetInjurySet extends EventWithBody {
    constructor(lineNumber: number, injury: boolean) {
        super("ScoreSheetInjurySet", { totalJamNumber: lineNumber, value: injury });
    }
}

export class ScoreSheetStarPassTripSet extends EventWithBody {
    constructor(teamSide: TeamSide, jamNumber: number, starPassTrip: number | null) {
        super("ScoreSheetStarPassTripSet", { teamSide, totalJamNumber: jamNumber, starPassTrip })
    }
}