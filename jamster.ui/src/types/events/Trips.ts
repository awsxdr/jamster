import { EventWithBody } from "@/hooks/EventsApi";
import { TeamSide } from "../TeamSide";

export class LastTripDeleted extends EventWithBody {
    constructor(team: TeamSide) {
        super("LastTripDeleted", { side: team });
    }
}
