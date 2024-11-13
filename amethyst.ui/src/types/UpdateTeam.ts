import { DisplayColor } from "./DisplayColor"
import { StringMap } from "./StringMap"

export type UpdateTeam = {
    names: StringMap<string>,
    colors: StringMap<StringMap<DisplayColor>>,
}