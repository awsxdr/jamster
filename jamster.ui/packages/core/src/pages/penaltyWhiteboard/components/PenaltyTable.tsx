import { CSSProperties, useMemo } from "react";
import { useI18n, usePenaltySheetState, useRulesState, useTeamDetailsState } from "@/hooks";
import { TeamSide } from "@/types";
import { PenaltyRow } from ".";

type PenaltyTableProps = {
    teamSide: TeamSide;
}

export const PenaltyTable = ({ teamSide }: PenaltyTableProps) => {

    const { rules } = useRulesState() ?? { };
    const { translate } = useI18n({ prefix: "PenaltyWhiteboard.PenaltyTable." });
    const penaltySheet = usePenaltySheetState(teamSide) ?? { lines: [] };

    const { team } = useTeamDetailsState(teamSide) ?? {};

    const teamName = useMemo(() => {
        if(!team) {
            return '';
        }

        return team.names['scoreboard'] 
            || team.names['team'] 
            || team.names['league'] 
            || team.names['color'] 
            || (teamSide === TeamSide.Home ? translate("Home") : translate("Away"));
    }, [team]);

    const skaterNumbers = team?.roster.filter(s => s.isSkating).map(s => s.number).sort() ?? [];
    const penaltyLines = useMemo(() => penaltySheet.lines.filter(l => skaterNumbers.includes(l.skaterNumber)), [skaterNumbers]);

    if (!rules) {
        return (<></>);
    }

    const columns = `repeat(${rules.penaltyRules.foulOutPenaltyCount + 5}, 1fr)`;

    return (
        <div 
            className="w-full grid grid-flow-cols grid-cols-[--plt-columns]"
            style={{ "--plt-columns": columns } as CSSProperties}
        >
            <div 
                className="col-start-1 col-end-[--col-end] row-start-1 border-b-2 border-black flex text-center justify-center items-center font-bold"
                style={{ '--col-end': rules?.penaltyRules.foulOutPenaltyCount + 4 } as CSSProperties}
            >
                {teamName}
            </div>
            <div className="col-start-11 row-start-1 font-bold flex text-center justify-center items-end border-b-2 border-black">
                <span className="2xl:hidden text-xs lg:text-base">{translate("FouloutExpulsion.Short")}</span>
                <span className="hidden 2xl:inline p-1">{translate("FouloutExpulsion.Long")}</span>
            </div>
            <div className="col-start-12 row-start-1 font-bold flex text-center justify-center items-end border-b-2 border-black">
                <span className="sm:hidden">Σ</span>
                <span className="hidden sm:inline 2xl:hidden text-xs lg:text-base">{translate("TotalPenalties.Short")}</span>
                <span className="hidden 2xl:inline p-1">{translate("TotalPenalties.Long")}</span>
            </div>
            { penaltyLines.map((line, row) => (
                <PenaltyRow 
                    key={line.skaterNumber} 
                    {...line} 
                    row={row}
                />) 
            )}
            <div 
                className="col-start-1 col-end-[--col-end] row-start-[--row] border-t border-black"
                style={{
                    '--col-end': rules?.penaltyRules.foulOutPenaltyCount + 7,
                    '--row': penaltyLines.length + 2
                } as CSSProperties}
            >
            </div>
        </div>
    );
}