import { Button, ButtonProps } from "@/components/ui";
import { useI18n, useJamStatsState } from "@/hooks";
import { useShortcut } from "@/hooks/InputControls";
import { cn } from "@/lib/utils";
import { InputControls, TeamSide } from "@/types";
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

    const shortcutGroup: keyof InputControls = side === TeamSide.Home ? "homeStats" : "awayStats";
    useShortcut(shortcutGroup, "lead", handleLead);
    useShortcut(shortcutGroup, "lost", handleLost);
    useShortcut(shortcutGroup, "called", handleCall);
    useShortcut(shortcutGroup, "starPass", handleStarPass);
    useShortcut(shortcutGroup, "initialTrip", handleInitialTrip);

    return (
        <>
            <div className="flex flex-wrap w-full justify-center items-center gap-2 p-5">
                <StatsButton active={jamStats?.lead} disabled={disabled} onClick={handleLead}>{translate("TripStats.Lead")}</StatsButton>
                <StatsButton active={jamStats?.lost} disabled={disabled} onClick={handleLost}>{translate("TripStats.Lost")}</StatsButton>
                <StatsButton active={jamStats?.called} disabled={disabled} onClick={handleCall}>{translate("TripStats.Call")}</StatsButton>
                <StatsButton active={jamStats?.starPass} disabled={disabled} onClick={handleStarPass}>{translate("TripStats.StarPass")}</StatsButton>
                <StatsButton active={jamStats?.hasCompletedInitial} disabled={disabled} onClick={handleInitialTrip}>{translate("TripStats.InitialComplete")}</StatsButton>
            </div>
        </>
    );
}