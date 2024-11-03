import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { useTeamDetailsState, useTripScoreState } from "@/hooks";
import { TeamSide } from "@/types"
import { useMemo } from "react";
import { TeamScore } from "./TeamScore";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { TripScore } from "./TripScore";

type TeamControlsProps = {
    side: TeamSide;
}

export const TeamControls = ({ side }: TeamControlsProps) => {

    const team = useTeamDetailsState(side);
    const teamName = useMemo(() => {
        if(!team) {
            return '';
        }

        return team.team.names['controls'] || team.team.names['default'] || '';
    }, [team]);

    const tripScore = useTripScoreState(side);

    return (
        <Card className="grow inline-block m-2">
            <CardHeader>
                <CardTitle className="text-center text-xl">{teamName}</CardTitle>
            </CardHeader>
            <CardContent>
                <Separator />
                <div className="flex w-full justify-center items-center">
                    <Button variant="secondary" className="text-md lg:text-xl">-1 [{side === TeamSide.Home ? 'a' : '\''}]</Button>
                    <TeamScore side={side} />
                    <Button variant="secondary" className="text-md lg:text-xl" >+1 [{side === TeamSide.Home ? 's' : '#'}]</Button>
                </div>
                <Separator />
                <TripScore tripScore={tripScore?.score ?? 0} />
            </CardContent>
        </Card>
    )
}