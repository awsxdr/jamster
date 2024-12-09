import { Button } from "@/components/ui";
import { TeamSide } from "@/types";

type TripStatsProps = {
    side: TeamSide;
    disabled?: boolean;
}

export const TripStats = ({ side, disabled }: TripStatsProps) => {
    return (
        <>
            <div className="flex flex-wrap w-full justify-center items-center gap-2 p-5">
                <Button variant="default" disabled={disabled}>Initial trip [{side === TeamSide.Home ? "d" : ";"}]</Button>
                <Button variant="secondary" disabled={disabled}>Lost [{side === TeamSide.Home ? "🠅d" : "🠅;"}]</Button>
                <Button variant="secondary" disabled={disabled}>Star pass [{side === TeamSide.Home ? "x" : "/"}]</Button>
            </div>
        </>
    );
}