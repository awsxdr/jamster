import { GameSkater, StringMap, TeamColor } from "./";

export type GameTeam = {
    names: StringMap<string>;
    color: TeamColor;
    roster: GameSkater[];
}