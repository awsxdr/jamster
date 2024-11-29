import { TeamColor } from "./TeamColor"
import { StringMap } from "./StringMap"

export type UpdateTeam = {
    names: StringMap<string>,
    colors: StringMap<TeamColor>,
}