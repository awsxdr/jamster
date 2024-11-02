import { DisplayColor, Skater, StringMap } from ".";

export type Team = {
    id: string,
    names: StringMap<string>,
    colors: StringMap<DisplayColor>,
    roster: Skater[],
};
