import { Button, ButtonProps } from "@/components/ui";
import { useI18n, useJamStatsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { TeamSide } from "@/types";
import { PropsWithChildren } from "react";

type StatsButtonProps = {
    active?: boolean;
    prominent?: boolean;
    className?: string;
} & ButtonProps;

const StatsButton = ({ active, prominent, className, children, ...props }: PropsWithChildren<StatsButtonProps>) => {
    return (
        <Button
            variant={prominent ? 'secondary' : 'outline'}
            className={cn(className, "border-2", active && "border-primary")}
            {...props}
        >
            {children}
        </Button>
    )
}

type TripStatsProps = {
    side: TeamSide;
    disabled?: boolean;
    onLeadChanged?: (side: TeamSide, value: boolean) => void;
    onLostChanged?: (side: TeamSide, value: boolean) => void;
    onCallChanged?: (side: TeamSide, value: boolean) => void;
    onStarPassChanged?: (side: TeamSide, value: boolean) => void;
    onInitialPassChanged?: (side: TeamSide, value: boolean) => void;
}

export const TripStats = ({ side, disabled, onLeadChanged, onLostChanged, onCallChanged, onStarPassChanged, onInitialPassChanged }: TripStatsProps) => {

    const jamStats = useJamStatsState(side);
    const { translate } = useI18n();

    return (
        <>
            <div className="flex flex-wrap w-full justify-center items-center gap-2 p-5">
                <StatsButton active={jamStats?.lead} disabled={disabled} onClick={() => onLeadChanged?.(side, !jamStats?.lead)}>{translate("TripStats.Lead")} [{side === TeamSide.Home ? "d" : ";"}]</StatsButton>
                <StatsButton active={jamStats?.lost} disabled={disabled} onClick={() => onLostChanged?.(side, !jamStats?.lost)}>{translate("TripStats.Lost")} [{side === TeamSide.Home ? "🠅d" : "🠅;"}]</StatsButton>
                <StatsButton active={jamStats?.called} disabled={disabled} onClick={() => onCallChanged?.(side, !jamStats?.called)}>{translate("TripStats.Call")} [{side === TeamSide.Home ? "r" : "o"}]</StatsButton>
                <StatsButton active={jamStats?.starPass} disabled={disabled} onClick={() => onStarPassChanged?.(side, !jamStats?.starPass)}>{translate("TripStats.StarPass")} [{side === TeamSide.Home ? "x" : "/"}]</StatsButton>
                <StatsButton active={jamStats?.hasCompletedInitial} disabled={disabled} onClick={() => onInitialPassChanged?.(side, !jamStats?.hasCompletedInitial)}>{translate("TripStats.InitialComplete")} [{side === TeamSide.Home ? "w" : "]"}]</StatsButton>
            </div>
        </>
    );
}