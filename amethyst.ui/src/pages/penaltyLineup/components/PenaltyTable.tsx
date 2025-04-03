import { CSSProperties, useMemo, useState } from "react";
import { useEvents, useI18n, usePenaltySheetState, useRulesState, useTeamDetailsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { Penalty, StringMap, TeamSide } from "@/types";
import { ExpulsionDialog, PenaltyDialog, PenaltyRow } from ".";
import { ExpulsionCleared, PenaltyAssessed, PenaltyRescinded, PenaltyUpdated, SkaterExpelled } from "@/types/events";

type PenaltyTableProps = {
    gameId: string;
    teamSide: TeamSide;
    offsetForLineupTable?: boolean;
    compact?: boolean;
}

export const PenaltyTable = ({ gameId, teamSide, offsetForLineupTable, compact }: PenaltyTableProps) => {

    const { rules } = useRulesState() ?? { };
    const { translate } = useI18n({ prefix: "PenaltyLineup.PenaltyTable." });
    const penaltySheet = usePenaltySheetState(teamSide) ?? { lines: [] };
    const { sendEvent } = useEvents();
    const { team } = useTeamDetailsState(teamSide) ?? {};

    const [penaltyDialogOpen, setPenaltyDialogOpen] = useState(false);
    const [expulsionDialogOpen, setExpulsionDialogOpen] = useState(false);
    const [editingSkaterNumber, setEditingSkaterNumber] = useState("");
    const [editingIndex, setEditingIndex] = useState(0);
    const [editingPenalty, setEditingPenalty] = useState<Penalty>();

    const skaterNumbers = team?.roster.filter(s => s.isSkating).map(s => s.number).sort() ?? [];
    const penaltyLines = useMemo(() => penaltySheet.lines.filter(l => skaterNumbers.includes(l.skaterNumber)), [skaterNumbers]);

    const skaterPenalties = useMemo(() => 
        penaltyLines.reduce((map, line) => ({ ...map, [line.skaterNumber]: line.penalties }), {} as StringMap<Penalty[]>), 
    [penaltyLines]);

    const skaterExpulsions = useMemo(() =>
        penaltyLines.reduce((map, line) => ({ ...map, [line.skaterNumber]: line.expulsionPenalty }), {} as StringMap<Penalty | null>),
    [penaltyLines]);

    if (!rules) {
        return (<></>);
    }
    
    const handlePenaltyClicked = (skaterNumber: string, index: number) => {
        const selectedSkaterPenalties = skaterPenalties[skaterNumber]!;
        setEditingSkaterNumber(skaterNumber);
        setEditingIndex(index);
        if(index < selectedSkaterPenalties.length) {
            setEditingPenalty(selectedSkaterPenalties[index]);
        } else {
            setEditingPenalty(undefined);
        }
        setPenaltyDialogOpen(true);
    }

    const handlePenaltyAccept = (penalty: Penalty) => {
        const selectedSkaterPenalties = [...skaterPenalties[editingSkaterNumber]!];
        if(editingIndex < selectedSkaterPenalties.length) {
            const originalPenalty = selectedSkaterPenalties[editingIndex];
            sendEvent(gameId, new PenaltyUpdated(
                teamSide, 
                editingSkaterNumber, 
                originalPenalty.code, 
                originalPenalty.period, 
                originalPenalty.jam,
                penalty.code,
                penalty.period,
                penalty.jam
            ));
        } else {
            sendEvent(gameId, new PenaltyAssessed(teamSide, editingSkaterNumber, penalty.code));
        }
    }

    const handlePenaltyDelete = () => {
        const selectedSkaterPenalties = [...skaterPenalties[editingSkaterNumber]!];
        if(editingIndex >= selectedSkaterPenalties.length) {
            return;
        }

        const penalty = selectedSkaterPenalties[editingIndex];
        sendEvent(gameId, new PenaltyRescinded(
            teamSide, 
            editingSkaterNumber, 
            penalty.code, 
            penalty.period, 
            penalty.jam));
    }

    const handleExpulsionClicked = (skaterNumber: string) => {
        setEditingSkaterNumber(skaterNumber);
        setExpulsionDialogOpen(true);
    }

    const handleExpulsionAccept = (penalty: Penalty | null) => {
        if(penalty === null) {
            sendEvent(gameId, new ExpulsionCleared(teamSide, editingSkaterNumber));
        } else {
            sendEvent(gameId, new SkaterExpelled(teamSide, editingSkaterNumber, penalty.code, penalty.period, penalty.jam));
        }
    }

    return (
        <>
            <div 
                className={cn(offsetForLineupTable ? "col-start-8" : "col-start-2", " col-end-[--col-end] row-start-1", "border-b-2 border-black")}
                style={{ '--col-end': rules?.penaltyRules.foulOutPenaltyCount + 2 + (offsetForLineupTable ? 8 : 3) } as CSSProperties}
            >
            </div>
            <div className={cn(offsetForLineupTable ? "col-start-[17]" : "col-start-12", "row-start-1", "font-bold flex text-center justify-center items-end", "border-b-2 border-black")}>
                <span className={cn("hidden sm:inline", !compact && "2xl:hidden", "text-xs lg:text-base")}>{translate("FouloutExpulsion.Short")}</span>
                <span className={cn("hidden", !compact && "2xl:inline")}>{translate("FouloutExpulsion.Long")}</span>
            </div>
            <div className={cn(offsetForLineupTable ? "col-start-[18]" : "col-start-13", "row-start-1", "font-bold flex text-center justify-center items-end", "border-b-2 border-black")}>
                <span className={cn("hidden sm:inline", !compact && "2xl:hidden", "text-xs lg:text-base")}>{translate("TotalPenalties.Short")}</span>
                <span className={cn("hidden", !compact && "2xl:inline")}>{translate("TotalPenalties.Long")}</span>
            </div>
            { penaltyLines.map((l, row) => (
                <PenaltyRow 
                    key={l.skaterNumber} 
                    {...l} 
                    row={row} 
                    offsetForLineupTable={offsetForLineupTable} 
                    compact={compact}
                    onPenaltyClicked={i => handlePenaltyClicked(l.skaterNumber, i)}
                    onExpulsionClicked={() => handleExpulsionClicked(l.skaterNumber)}
                />) 
            )}
            <div 
                className={cn(offsetForLineupTable ? "col-start-8" : "col-start-2", "col-end-[--col-end] row-start-[--row]", "border-t border-black")}
                style={{
                    '--col-end': rules?.penaltyRules.foulOutPenaltyCount + 2 + (offsetForLineupTable ? 8 : 3) + 2
                } as CSSProperties}
            >
            </div>
            <PenaltyDialog 
                open={penaltyDialogOpen} 
                currentPenalty={editingPenalty}
                onOpenChanged={setPenaltyDialogOpen} 
                onAccept={handlePenaltyAccept} 
                onDelete={handlePenaltyDelete} 
            />
            <ExpulsionDialog
                open={expulsionDialogOpen}
                expulsionPenalty={skaterExpulsions[editingSkaterNumber] ?? null}
                penalties={skaterPenalties[editingSkaterNumber] ?? []}
                onOpenChanged={setExpulsionDialogOpen}
                onAccept={handleExpulsionAccept}
            />
        </>
    );
}