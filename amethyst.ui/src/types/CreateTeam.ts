import { DisplayColor } from "./DisplayColor"
import { StringMap } from "./StringMap"

export type CreateTeam = {
    names: StringMap<string>,
    colors: StringMap<DisplayColor>,
}