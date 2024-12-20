import { EventWithBody } from "@/hooks/EventsApiHook";

export class IntermissionClockSet extends EventWithBody {
    constructor(secondsRemaining: number) {
        super("IntermissionClockSet", { secondsRemaining });
    }
}

export class JamClockSet extends EventWithBody {
    constructor(secondsRemaining: number) {
        super("JamClockSet", { secondsRemaining });
    }
}

export class LineupClockSet extends EventWithBody {
    constructor(secondsPassed: number) {
        super("LineupClockSet", { secondsPassed });
    }
}

export class PeriodClockSet extends EventWithBody {
    constructor(secondsRemaining: number) {
        super("PeriodClockSet", { secondsRemaining });
    }
}

export class TimeoutClockSet extends EventWithBody {
    constructor(secondsPassed: number) {
        super("TimeoutClockSet", { secondsPassed });
    }
}