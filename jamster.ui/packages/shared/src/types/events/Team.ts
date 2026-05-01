import { EventWithBody } from "./Event";
import { GameTeam, TeamSide } from "../";

export class TeamSet extends EventWithBody {
    constructor(teamSide: TeamSide, team: GameTeam) {
        super("TeamSet", { teamSide, team });
    }
}
