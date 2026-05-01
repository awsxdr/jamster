import { EventWithBody } from "./Event";
import { TeamSide } from "../TeamSide";

export class LastTripDeleted extends EventWithBody {
    constructor(team: TeamSide) {
        super("LastTripDeleted", { side: team });
    }
}
