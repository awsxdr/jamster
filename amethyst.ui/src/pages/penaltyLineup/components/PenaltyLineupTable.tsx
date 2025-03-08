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

    const columnCount =
        display === "Both" ? 17
        : display === "Lineup" ? 6
        : display === "Penalties" ? 12
        : 0;

    const columns = `auto repeat(${columnCount}, 1fr)`

    return (
        <>
            <div className="w-full text-center font-bold">{teamName}</div>
            <div 
                className={cn(
                    "w-full grid grid-flow-cols grid-cols-[--plt-columns]",
                )}
                style={{ "--plt-columns": columns } as CSSProperties}
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