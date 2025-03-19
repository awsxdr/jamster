import { Clock } from "@/components/clocks/Clock";
import { Button, Card, CardContent, Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui";
import { useBoxTripsState, useJamLineupState, useTeamDetailsState } from "@/hooks";
import { BoxTrip, TeamSide } from "@/types";
import { EllipsisVertical, Pause, Square } from "lucide-react";
import { useMemo, useState } from "react";

type BoxTripItemProps = {
    teamSide: TeamSide;
    trip: BoxTrip;
}

const BoxTripItem = ({ trip, teamSide }: BoxTripItemProps) => {

    const lineup = useJamLineupState(teamSide);

    const [skaterNumber, setSkaterNumber] = useState(trip.skaterNumber);

    const onTrackSkaters = useMemo(() => [
        lineup?.jammerNumber,
        lineup?.pivotNumber,
        ...(lineup?.blockerNumbers ?? [])
    ].filter(s => s).sort((a, b) => a!.localeCompare(b!)) as string[], [lineup]);

    if(!lineup) {
        return (<></>);
    }

    return (
        <Card>
            <CardContent className="flex items-center p-4 pr-2 gap-2">
                {/* <Select value={skaterNumber} onValueChange={setSkaterNumber}>
                    <SelectTrigger className="w-20">
                        <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem value="?">?</SelectItem>
                        {onTrackSkaters.map(s => (
                            <SelectItem key={s} value={s}>{s}</SelectItem>
                        ))}
                    </SelectContent>
                </Select> */}
                <div>
                    #{trip.skaterNumber}
                </div>
                <Clock 
                    state={trip}
                    secondsMapper={t => t.secondsPassed}
                    direction="up"
                    textClassName="text-2xl px-2"
                />
                <Button variant="outline" disabled={trip.durationInJams !== null} size="icon"><Pause /></Button>
                <Button variant="default" disabled={trip.durationInJams !== null} size="icon"><Square /></Button>
                <Button variant="ghost" size="icon" className="w-auto ml-2 px-0"><EllipsisVertical /></Button>
            </CardContent>
        </Card>
    );
}

type BoxTripListProps = {
    teamSide: TeamSide;
}

export const BoxTripList = ({ teamSide }: BoxTripListProps) => {

    const { boxTrips } = useBoxTripsState(teamSide) ?? { boxTrips: [] };

    return (
        <div className="flex">
            { boxTrips.map((trip, i) => (
                <BoxTripItem 
                    key={i}
                    trip={trip} 
                    teamSide={teamSide}
                />
            ))}
        </div>
    );
}