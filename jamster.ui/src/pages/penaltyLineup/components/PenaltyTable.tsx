import { CSSProperties, useMemo, useState } from "react";
import { eventsApi, useI18n, usePenaltySheetState, useRulesState, useTeamDetailsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { Penalty, StringMap, TeamSide } from "@/types";
import { ExpulsionDialog, PenaltyDialog, PenaltyRow } from ".";
import { ExpulsionCleared, PenaltyAssessed, PenaltyRescinded, PenaltyServedSet, PenaltyUpdated, SkaterExpelled } from "@/types/events";

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
    const { team } = useTeamDetailsState(teamSide) ?? {};

    const [penaltyDialogOpen, setPenaltyDialogOpen] = useState(false);
    const [expulsionDialogOpen, setExpulsionDialogOpen] = useState(false);
    const [editingSkaterId, setEditingSkaterId] = useState("");
    const [editingIndex, setEditingIndex] = useState(0);
    const [editingPenalty, setEditingPenalty] = useState<Penalty>();

    const skaterIds = team?.roster.filter(s => s.isSkating).sort((a, b) => a.number.localeCompare(b.number)).map(s => s.id) ?? [];
    const penaltyLines = useMemo(() => penaltySheet.lines.filter(l => skaterIds.includes(l.skaterId)), [skaterIds]);

    const skaterPenalties = useMemo(() => 
        penaltyLines.reduce((map, line) => ({ ...map, [line.skaterId]: line.penalties }), {} as StringMap<Penalty[]>), 
    [penaltyLines]);

    const skaterExpulsions = useMemo(() =>
        penaltyLines.reduce((map, line) => ({ ...map, [line.skaterId]: line.expulsionPenalty }), {} as StringMap<Penalty | null>),
    [penaltyLines]);

    if (!rules) {
        return (<></>);
    }
    
    const handlePenaltyClicked = (skaterId: string, index: number) => {
        const selectedSkaterPenalties = skaterPenalties[skaterId] ?? [];
        setEditingSkaterId(skaterId);
        setEditingIndex(index);
        if(index < selectedSkaterPenalties.length) {
            setEditingPenalty(selectedSkaterPenalties[index]);
        } else {
            setEditingPenalty(undefined);
        }
        setPenaltyDialogOpen(true);
    }

    const handlePenaltyAccept = (penalty: Penalty) => {
        const selectedSkaterPenalties = [...(skaterPenalties[editingSkaterId] ?? [])];
        if(editingIndex < selectedSkaterPenalties.length) {
            (async () => {
                const originalPenalty = selectedSkaterPenalties[editingIndex];
                eventsApi.sendEvent(gameId, new PenaltyUpdated(
                    teamSide, 
                    editingSkaterId, 
                    originalPenalty.code, 
                    originalPenalty.period, 
                    originalPenalty.jam,
                    penalty.code,
                    penalty.period,
                    penalty.jam
                ));
                if(originalPenalty.served !== penalty.served) {
                    eventsApi.sendEvent(gameId, new PenaltyServedSet(teamSide, editingSkaterId, penalty.code, penalty.period, penalty.jam, penalty.served));
                }
            })();
        } else {
            eventsApi.sendEvent(gameId, new PenaltyAssessed(teamSide, editingSkaterId, penalty.code));
        }
    }

    const handlePenaltyDelete = () => {
        const selectedSkaterPenalties = [...(skaterPenalties[editingSkaterId] ?? [])];
        if(editingIndex >= selectedSkaterPenalties.length) {
            return;
        }

        const penalty = selectedSkaterPenalties[editingIndex];
        eventsApi.sendEvent(gameId, new PenaltyRescinded(
            teamSide, 
            editingSkaterId, 
            penalty.code, 
            penalty.period, 
            penalty.jam));
    }

    const handleExpulsionClicked = (skaterId: string) => {
        setEditingSkaterId(skaterId);
        setExpulsionDialogOpen(true);
    }

    const handleExpulsionAccept = (penalty: Penalty | null) => {
        if(penalty === null) {
            eventsApi.sendEvent(gameId, new ExpulsionCleared(teamSide, editingSkaterId));
        } else {
            eventsApi.sendEvent(gameId, new SkaterExpelled(teamSide, editingSkaterId, penalty.code, penalty.period, penalty.jam));
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
                    key={l.skaterId} 
                    {...l} 
                    row={row} 
                    offsetForLineupTable={offsetForLineupTable} 
                    compact={compact}
                    onPenaltyClicked={i => handlePenaltyClicked(l.skaterId, i)}
                    onExpulsionClicked={() => handleExpulsionClicked(l.skaterId)}
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
                expulsionPenalty={skaterExpulsions[editingSkaterId] ?? null}
                penalties={skaterPenalties[editingSkaterId] ?? []}
                onOpenChanged={setExpulsionDialogOpen}
                onAccept={handleExpulsionAccept}
            />
        </>
    );
}