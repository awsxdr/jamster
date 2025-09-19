import { TeamColor, Skater, StringMap } from ".";

export type Team = {
    id: string,
    names: StringMap<string>,
    colors: StringMap<TeamColor>,
    roster: Skater[],
};
