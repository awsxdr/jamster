import { DisplayColor, Skater, StringMap } from ".";

export type Team = {
    id: string,
    names: StringMap<string>,
    colors: StringMap<StringMap<DisplayColor>>,
    roster: Skater[],
};
