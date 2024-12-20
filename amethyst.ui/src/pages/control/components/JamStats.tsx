import { Button, ButtonProps } from "@/components/ui";
import { useI18n, useJamStatsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { TeamSide } from "@/types";
import { PropsWithChildren } from "react";
import { useHotkeys } from "react-hotkeys-hook";

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

type JamStatsProps = {
    side: TeamSide;
    disabled?: boolean;
    onLeadChanged?: (side: TeamSide, value: boolean) => void;
    onLostChanged?: (side: TeamSide, value: boolean) => void;
    onCallChanged?: (side: TeamSide, value: boolean) => void;
    onStarPassChanged?: (side: TeamSide, value: boolean) => void;
    onInitialPassChanged?: (side: TeamSide, value: boolean) => void;
}

export const JamStats = ({ side, disabled, onLeadChanged, onLostChanged, onCallChanged, onStarPassChanged, onInitialPassChanged }: JamStatsProps) => {

    const jamStats = useJamStatsState(side);
    const { translate } = useI18n();

    const handleLead = () => onLeadChanged?.(side, !jamStats?.lead);
    const handleLost = () => onLostChanged?.(side, !jamStats?.lost);
    const handleCall = () => onCallChanged?.(side, !jamStats?.called);
    const handleStarPass = () => onStarPassChanged?.(side, !jamStats?.starPass);
    const handleInitialTrip = () => onInitialPassChanged?.(side, !jamStats?.hasCompletedInitial);

    useHotkeys(side === TeamSide.Home ? 'd' : ';', handleLead, { preventDefault: true });
    useHotkeys(side === TeamSide.Home ? 'shift+d' : 'shift+semicolon', handleLost, { preventDefault: true });
    useHotkeys(side === TeamSide.Home ? 'r' : 'o', handleCall, { preventDefault: true });
    useHotkeys(side === TeamSide.Home ? 'x' : '/', handleStarPass, { preventDefault: true });
    useHotkeys(side === TeamSide.Home ? 'w' : ']', handleInitialTrip, { preventDefault: true });

    return (
        <>
            <div className="flex flex-wrap w-full justify-center items-center gap-2 p-5">
                <StatsButton active={jamStats?.lead} disabled={disabled} onClick={handleLead}>{translate("TripStats.Lead")} [{side === TeamSide.Home ? "d" : ";"}]</StatsButton>
                <StatsButton active={jamStats?.lost} disabled={disabled} onClick={handleLost}>{translate("TripStats.Lost")} [{side === TeamSide.Home ? "🠅d" : "🠅;"}]</StatsButton>
                <StatsButton active={jamStats?.called} disabled={disabled} onClick={handleCall}>{translate("TripStats.Call")} [{side === TeamSide.Home ? "r" : "o"}]</StatsButton>
                <StatsButton active={jamStats?.starPass} disabled={disabled} onClick={handleStarPass}>{translate("TripStats.StarPass")} [{side === TeamSide.Home ? "x" : "/"}]</StatsButton>
                <StatsButton active={jamStats?.hasCompletedInitial} disabled={disabled} onClick={handleInitialTrip}>{translate("TripStats.InitialComplete")} [{side === TeamSide.Home ? "w" : "]"}]</StatsButton>
            </div>
        </>
    );
}