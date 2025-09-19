import { ShortcutButton, ShortcutButtonProps } from "@/components";
import { useI18n, useJamStatsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { InputControls, TeamSide } from "@/types";
import { PropsWithChildren } from "react";

type StatsButtonProps<TGroupKey extends keyof InputControls, TControlKey extends keyof InputControls[TGroupKey]> = {
    active?: boolean;
    prominent?: boolean;
    className?: string;
} & ShortcutButtonProps<TGroupKey, TControlKey>;

const StatsButton = <TGroupKey extends keyof InputControls, TControlKey extends keyof InputControls[TGroupKey]>({ active, prominent, className, children, ...props }: PropsWithChildren<StatsButtonProps<TGroupKey, TControlKey>>) => {
    return (
        <ShortcutButton
            variant={prominent ? 'secondary' : 'outline'}
            className={cn(className, "border-2", active && "border-primary")}
            {...props}
        >
            {children}
        </ShortcutButton>
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
    const { translate } = useI18n({ prefix: "ScoreboardControl.JamStats." });

    const handleLead = () => onLeadChanged?.(side, !jamStats?.lead);
    const handleLost = () => onLostChanged?.(side, !jamStats?.lost);
    const handleCall = () => onCallChanged?.(side, !jamStats?.called);
    const handleStarPass = () => onStarPassChanged?.(side, !jamStats?.starPass);
    const handleInitialTrip = () => onInitialPassChanged?.(side, !jamStats?.hasCompletedInitial);

    const shortcutGroup: keyof InputControls = side === TeamSide.Home ? "homeStats" : "awayStats";

    return (
        <>
            <div className="flex flex-wrap w-full justify-center items-center gap-2 py-2">
                <StatsButton 
                    description={translate("Lead.Tooltip")} 
                    active={jamStats?.lead} 
                    disabled={disabled}
                    shortcutGroup={shortcutGroup}
                    shortcutKey="lead"
                    onClick={handleLead}
                >
                    {translate("Lead")}
                </StatsButton>
                <StatsButton 
                    description={translate("Lost.Tooltip")} 
                    active={jamStats?.lost} 
                    disabled={disabled}
                    shortcutGroup={shortcutGroup}
                    shortcutKey="lost"
                    onClick={handleLost}
                >
                    {translate("Lost")}
                </StatsButton>
                <StatsButton 
                    description={translate("Call.Tooltip")} 
                    active={jamStats?.called} 
                    disabled={disabled} 
                    shortcutGroup={shortcutGroup}
                    shortcutKey="called"
                    onClick={handleCall}
                >
                    {translate("Call")}
                </StatsButton>
                <StatsButton 
                    description={translate("StarPass.Tooltip")} 
                    active={jamStats?.starPass} 
                    disabled={disabled} 
                    shortcutGroup={shortcutGroup}
                    shortcutKey="starPass"
                    onClick={handleStarPass}
                >
                    {translate("StarPass")}
                </StatsButton>
                <StatsButton 
                    description={translate("InitialComplete.Tooltip")} 
                    active={jamStats?.hasCompletedInitial} 
                    disabled={disabled} 
                    shortcutGroup={shortcutGroup}
                    shortcutKey="initialTrip"
                    onClick={handleInitialTrip}
                >
                    {translate("InitialComplete")}
                </StatsButton>
            </div>
        </>
    );
}