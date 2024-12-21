import { StringMap, TeamColor } from "@/types";

export const useTeamColorMap = (): StringMap<TeamColor> => [
    [["red"], ["#ff0000", "#ffffff"]],
    [["pink"], ["#ff8888", "#000000"]],
    [["orange"], ["#ff8800", "#000000"]],
    [["yellow"], ["#ffff00", "#000000"]],
    [["gold"], ["#888800", "#000000"]],
    [["brown"], ["#884400", "#ffffff"]],
    [["lime"], ["#88ff00", "#000000"]],
    [["green"], ["#00aa00", "#ffffff"]],
    [["teal", "turquoise"], ["#008888", "#000000"]],
    [["blue"], ["#0000ff", "#ffffff"]],
    [["purple"], ["#8800ff", "#ffffff"]],
    [["black"], ["#000000", "#ffffff"]],
    [["grey", "gray"], ["#666666", "#ffffff"]],
    [["white"], ["#ffffff", "#000000"]],
]
.flatMap(x => x[0].map(name => ({ name, shirtColor: x[1][0], complementaryColor: x[1][1] })))
.reduce((map, { name, ...current }) => ({ ...map, [name]: current}), {});

