import { TeamSide } from "@/types";
import { TeamName } from "./TeamName";

import { TeamTimeouts } from "./TeamTimeouts";
import { TeamScore } from "./TeamScore";
import { JamScore } from "./JamScore";
import { cn } from "@/lib/utils";

type TeamDetailsProps = {
    side: TeamSide
}

export const TeamDetails = ({ side }: TeamDetailsProps) => {
    return (
        <div className={cn("flex flex-col grow w-1/2", side === TeamSide.Home ? "ml-5" : "mr-5")}>
            <div className="h-[40%] mb-5">
                <TeamName side={side} textClassName={side === TeamSide.Home ? "" : "text-black bg-white"} />
            </div>
            <div className={cn("flex h-[calc(60%-1.4rem)] gap-5", side === TeamSide.Home ? "flex-row" : "flex-row-reverse")}>
                <TeamTimeouts side={side} />
                <TeamScore side={side} />
                <JamScore side={side} />
            </div>
        </div>
    );
}