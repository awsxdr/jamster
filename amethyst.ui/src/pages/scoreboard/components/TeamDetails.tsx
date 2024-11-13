import { TeamSide } from "@/types";
import { TeamName } from "./TeamName";

import { TeamTimeouts } from "./TeamTimeouts";
import { TeamScore } from "./TeamScore";
import { TripScore } from "./TripScore";
import { cn } from "@/lib/utils";

type TeamDetailsProps = {
    side: TeamSide
}

export const TeamDetails = ({ side }: TeamDetailsProps) => {
    return (
        <div className="flex flex-col grow w-1/2">
            <div className="h-[40%]">
                <TeamName side={side} />
            </div>
            <div className={cn("flex h-[60%]", side === TeamSide.Home ? "flex-row" : "flex-row-reverse")}>
                <TeamTimeouts side={side} />
                <TeamScore side={side} />
                <TripScore side={side} />
            </div>
        </div>
    );
}