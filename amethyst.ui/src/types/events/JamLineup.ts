import { EventWithBody } from "@/hooks/EventsApiHook";
import { TeamSide } from "../TeamSide"

export class SkaterOnTrack extends EventWithBody {
    constructor(side: TeamSide, position: SkaterPosition, skaterNumber: string | null) {
        super("SkaterOnTrack", { side, skaterNumber, position });
    }
}

export type SkaterOnTrackBody = {
    side: TeamSide;
    skaterNumber: string | null;
    position: SkaterPosition;
}

export enum SkaterPosition {
    Jammer = 'Jammer',
    Pivot = 'Pivot',
}