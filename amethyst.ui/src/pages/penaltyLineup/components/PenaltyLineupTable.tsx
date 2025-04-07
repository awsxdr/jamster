import { useGameStageState, useRulesState, useTeamDetailsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { TeamSide } from "@/types";
import { CSSProperties, useMemo } from "react";
import { LineupTable } from ".";
import { PenaltyTable } from "./PenaltyTable";

type PenaltyLineupDisplay = "Penalties" | "Lineup" | "Both";

type PenaltyLineupTableProps = {
    teamSide: TeamSide;
    gameId: string;
    compact?: boolean;
    display: PenaltyLineupDisplay;
}

export const PenaltyLineupTable = ({ teamSide, gameId, compact, display }: PenaltyLineupTableProps) => {
    const { team } = useTeamDetailsState(teamSide) ?? {};
    const gameStage = useGameStageState();
    const { rules } = useRulesState() ?? { };

    const teamName = useMemo(() => {
        if(!team) {
            return '';
        }

        return team.names['controls'] || team.names['color'] ||  team.names['team'] || team.names['league'] || '';
    }, [team]);

    if(!team || !gameStage || !rules) {
        return (<></>);
    }

    const lineupColumns = display === "Both" || display === "Lineup" ? 5 : 0;

    const penaltyColumns = display === "Both" || display === "Penalties" ? 11 : 0;

    const equalColumns = `auto 1fr repeat(${lineupColumns + penaltyColumns}, 1fr)`;
    const equalColumnsNoMenu = `0px 1fr repeat(${lineupColumns + penaltyColumns}, 1fr)`;
    const weightedColumns = `0px 2fr ${"3fr ".repeat(lineupColumns)} ${"2fr ".repeat(penaltyColumns)}`;

    return (
        <>
            <div className="w-full text-center font-bold">{teamName}</div>
            <div 
                className={cn(
                    "w-full grid grid-flow-cols grid-cols-[--plt-equal-columns-no-menu] sm:grid-cols-[--plt-weighted-columns] 2xl:grid-cols-[--plt-equal-columns]",
                )}
                style={{ 
                    "--plt-equal-columns-no-menu": equalColumnsNoMenu,
                    "--plt-equal-columns": equalColumns,
                    "--plt-weighted-columns": weightedColumns,
                } as CSSProperties}
            >
                { display !== "Penalties" && (
                    <LineupTable gameId={gameId} teamSide={teamSide} compact={compact} />
                )}
                { display !== "Lineup" && (
                    <PenaltyTable gameId={gameId} teamSide={teamSide} offsetForLineupTable={display === "Both"} compact={compact} />
                )}
            </div>
        </>
    )
}