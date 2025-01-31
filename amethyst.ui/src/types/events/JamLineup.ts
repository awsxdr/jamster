import { EventWithBody } from "@/hooks/EventsApiHook";
import { TeamSide } from "../TeamSide"

export class SkaterOnTrack extends EventWithBody {
    constructor(teamSide: TeamSide, position: SkaterPosition, skaterNumber: string | null) {
        super("SkaterOnTrack", { teamSide, skaterNumber, position });
    }
}

export type SkaterOnTrackBody = {
    teamSide: TeamSide;
    skaterNumber: string | null;
    position: SkaterPosition;
}

export enum SkaterPosition {
    Jammer = 'Jammer',
    Pivot = 'Pivot',
}