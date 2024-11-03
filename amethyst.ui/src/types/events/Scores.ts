import { EventWithBody } from "@/hooks/EventsApiHook";
import { TeamSide } from "../TeamSide";

export class ScoreModifiedRelative extends EventWithBody {
    constructor(body: ScoreModifiedRelativeBody) {
        super("ScoreModifiedRelative", body);
    }
}
type ScoreModifiedRelativeBody = {
    teamSide: TeamSide;
    value: number;
}