import { EventWithBody } from "@/hooks/EventsApi";
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