import { TeamSide } from "@/types";
import { TeamName } from "./TeamName";

import { TeamTimeouts } from "./TeamTimeouts";
import { TeamScore } from "./TeamScore";
import { JamScore } from "./JamScore";
import { cn } from "@/lib/utils";
import { ScaledText } from "@/components/ScaledText";
import { useI18n, useJamStatsState } from "@/hooks";
import { SCOREBOARD_GAP_CLASS_NAME } from "../Scoreboard";

type TeamDetailsProps = {
    side: TeamSide
}

export const TeamDetails = ({ side }: TeamDetailsProps) => {

    const { starPass } = useJamStatsState(side) ?? { starPass: false };
    const { translate } = useI18n({ prefix: "Scoreboard.TeamDetails." })

    return (
        <div className={cn("flex flex-col grow w-1/2", SCOREBOARD_GAP_CLASS_NAME)}>
            <div className="h-[40%]">
                <TeamName side={side} />
            </div>
            <div className={cn("flex h-[calc(60%-1.4rem)]", SCOREBOARD_GAP_CLASS_NAME, side === TeamSide.Home ? "flex-row" : "flex-row-reverse")}>
                <TeamTimeouts side={side} />
                <TeamScore side={side} />
                <div className="flex flex-col w-1/6 justify-between">
                    <JamScore side={side} />
                    <div className="text-white h-2/5">
                        <ScaledText 
                            text={starPass ? translate("StarPassInicator") : ""}
                            className={cn("flex justify-center items-center h-full overflow-hidden font-bold")} 
                        />
                    </div>
                </div>
            </div>
        </div>
    );
}