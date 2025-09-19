import { TeamColor } from "./TeamColor"
import { StringMap } from "./StringMap"

export type CreateTeam = {
    names: StringMap<string>,
    colors: StringMap<TeamColor>,
}