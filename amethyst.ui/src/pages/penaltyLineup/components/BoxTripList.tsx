import { useMemo } from "react";
import { useBoxTripsState } from "@/hooks";
import { TeamSide } from "@/types";
import { BoxDisplayType, BoxTripItem } from ".";
import { SkaterPosition } from "@/types/events";

type BoxTripListProps = {
    teamSide: TeamSide;
    boxDisplayType: BoxDisplayType;
}

export const BoxTripList = ({ teamSide, boxDisplayType }: BoxTripListProps) => {

    const { boxTrips } = useBoxTripsState(teamSide) ?? { boxTrips: [] };

    const visiblePositions =
        boxDisplayType === "Both" ? [SkaterPosition.Jammer, SkaterPosition.Pivot, SkaterPosition.Blocker]
        : boxDisplayType === "Jammers" ? [SkaterPosition.Jammer]
        : boxDisplayType === "Blockers" ? [SkaterPosition.Pivot, SkaterPosition.Blocker]
        : [];

    const orderedTrips = useMemo(() => {

        const filteredTrips = boxTrips.filter(t => visiblePositions.includes(t.skaterPosition));
        const reversedTrips = filteredTrips.reverse();

        return [
            ...reversedTrips.filter(b => b.durationInJams === null),
            ...reversedTrips.filter(b => b.durationInJams !== null),
        ];
    }, [boxTrips, visiblePositions]);

    return (
        <div className="flex flex-col flex-wrap gap-5 p-5">
            { orderedTrips.map((trip, i) => (
                <BoxTripItem 
                    key={i}
                    trip={trip} 
                    teamSide={teamSide}
                />
            ))}
        </div>
    );
}