import { EventWithBody } from "@/hooks/EventsApi";
import { GameTeam, TeamSide } from "@/types";

export class TeamSet extends EventWithBody {
    constructor(teamSide: TeamSide, team: GameTeam) {
        super("TeamSet", { teamSide, team });
    }
}
