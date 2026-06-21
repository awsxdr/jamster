import { Button } from "@/components/ui";
import { eventsApi, useGameStageState, useI18n, useInjuriesState, useLineupSheetState, usePenaltySheetState, useRulesState, useTeamDetailsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { LineupPosition, Penalty, StringMap, TeamSide } from "@/types";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { MenuColumn, SkaterNumberColumn } from ".";
import { useEffect, useMemo, useState } from "react";
import { SkaterPositionColumn } from "./SkaterPositionColumn";
import { SkaterAddedToJam, SkaterInjuryAdded, SkaterInjuryRemoved, SkaterPosition, SkaterReleasedFromBox, SkaterRemovedFromJam, SkaterSatInBox } from "@/types/events";
import { PenaltyBoxColumn } from "./PenaltyBoxColumn";
import { ternary } from "@/utilities/switchex";

type LineupTableProps = {
    gameId: string;
    teamSide: TeamSide,
    compact?: boolean;
}

export const LineupTable = ({
    gameId,
    teamSide,
    compact, 
}: LineupTableProps) => {

    const { translate } = useI18n({ prefix: "PenaltyLineup.LineupTable." })

    const { jams } = useLineupSheetState(teamSide) ?? { jams: [] };
    const { team } = useTeamDetailsState(teamSide) ?? {};
    const { injuries } = useInjuriesState(teamSide) ?? { injuries: [] };
    const penaltySheet = usePenaltySheetState(teamSide) ?? { lines: [] };
    const { rules } = useRulesState() ?? { };    

    const gameStage = useGameStageState();

    const [totalJamNumber, setTotalJamNumber] = useState(-1);

    useEffect(() => {
        if(totalJamNumber > -1 || !gameStage) {
            return;
        }

        setTotalJamNumber(jams.findIndex(j => j.jam === gameStage.jamNumber + 1 && j.period === gameStage.periodNumber));

    }, [jams, gameStage])

    const currentJam = useMemo(() => {
        if(totalJamNumber < 0 || totalJamNumber > jams.length) {
            return undefined;
        }

        return jams[totalJamNumber];
    }, [totalJamNumber, jams]);

    const skatingSkaters = team?.roster.filter(s => s.isSkating).sort((a, b) => a.number < b.number ? -1 : a.number > b.number ? 1 : 0) ?? [];

    const skaterPositions = useMemo(() =>
        team?.roster.reduce((map, { id }) => ({
            ...map,
            [id]: (
                ternary()
                    .predicate(_ => currentJam?.jammerId === id).then(LineupPosition.Jammer)
                    .predicate(_ => currentJam?.pivotId === id).then(LineupPosition.Pivot)
                    .predicate(_ => currentJam?.blockerIds.includes(id) ?? false).then(LineupPosition.Blocker)
                    .default(LineupPosition.Bench)
            )
        }), {} as StringMap<LineupPosition>),
    [currentJam, team]);

    const skaterPenalties = useMemo(() => 
        penaltySheet.lines.reduce((map, line) => ({ ...map, [line.skaterId]: line.penalties }), {} as StringMap<Penalty[]>), 
    [penaltySheet]);

    const skaterExpulsions = useMemo(() =>
        penaltySheet.lines.reduce((map, line) => ({ ...map, [line.skaterId]: line.expulsionPenalty }), {} as StringMap<Penalty | null>),
    [penaltySheet]);

    const offTrackSkaters = useMemo(() => 
        Object.keys(skaterPenalties).filter(n => 
            (skaterPenalties[n]?.length ?? 0) >= (rules?.penaltyRules.foulOutPenaltyCount ?? 0)
            || skaterExpulsions[n] !== null
        ),
    [skaterPenalties, skaterExpulsions, injuries, rules]);

    const injuredSkaters = useMemo(() =>
        injuries.filter(i => !i.expired).map(i => team?.roster.find(s => s.id == i.skaterId)?.number ?? ""), [injuries]);

    const handlePositionClicked = (skaterId: string, position: LineupPosition) => {
        if(!currentJam) {
            return;
        }

        if(position === LineupPosition.Bench) {
            eventsApi.sendEvent(gameId, new SkaterRemovedFromJam(
                teamSide, 
                currentJam.period,
                currentJam.jam,
                skaterId
            ));
        } else {
            eventsApi.sendEvent(gameId, new SkaterAddedToJam(
                teamSide,
                currentJam.period,
                currentJam.jam,
                position as unknown as SkaterPosition, 
                skaterId));
        }
    }

    const handleBoxClicked = (skaterId: string, currentlyInBox: boolean) => {
        if(currentlyInBox) {
            eventsApi.sendEvent(gameId, new SkaterReleasedFromBox(teamSide, skaterId));
        } else {
            eventsApi.sendEvent(gameId, new SkaterSatInBox(teamSide, skaterId));
        }
    }

    if(!skaterPositions) {
        return (<></>);
    }

    const headerClassName = "border-b-2 border-black";
    const headerTextClassName = "font-bold flex text-center justify-center items-end";

    const handleInjuryAdded = (skaterId: string) => {
        eventsApi.sendEvent(gameId, new SkaterInjuryAdded(teamSide, skaterId));
    }

    const handleInjuryRemoved = (skaterId: string) => {
        const injuryJamNumber = injuries.filter(i => i.skaterId === skaterId)[-1]?.totalJamNumberStart;

        if(injuryJamNumber === undefined) {
            return;
        }

        eventsApi.sendEvent(gameId, new SkaterInjuryRemoved(teamSide, skaterId, injuryJamNumber));
    }

    return (
        <>
            <div className={cn("col-start-2 row-start-1 flex items-end", headerClassName)}>
                <Button 
                    id="PenaltyLineup.PreviousJamButton"
                    className="w-full p-0 lg:p-2 rounded-none"
                    variant="secondary" 
                    disabled={totalJamNumber <= 0} 
                    onClick={() => setTotalJamNumber(t => t - 1)}
                >
                    <ChevronLeft />
                    <span className={cn("hidden", !compact && "lg:inline")}>{translate("PreviousJam")}</span>
                </Button>
            </div>
            <div id="PenaltyLineup.PeriodJamDisplay" className={cn("col-start-3 col-span-4 row-start-1 gap-2", headerClassName, headerTextClassName, "items-center")}>
                <span>
                    <span className={cn(!compact && "lg:hidden")}>{translate("Period.Short")}</span>
                    <span className={cn("hidden", !compact && "lg:inline")}>{translate("Period.Long")}</span>
                    { currentJam?.period }
                </span>
                <span>-</span>
                <span>
                    <span className={cn(!compact && "lg:hidden")}>{translate("Jam.Short")}</span>
                    <span className={cn("hidden", !compact && "lg:inline")}>{translate("Jam.Long")}</span>
                    { currentJam?.jam }
                </span>
            </div>
            <div className={cn("col-start-7 row-start-1 flex items-end", headerClassName)}>
                <Button 
                    id="PenaltyLineup.NextJamButton"
                    className="w-full p-0 lg:p-2 rounded-none"
                    disabled={totalJamNumber >= jams.length - 1 || totalJamNumber === -1} 
                    onClick={() => setTotalJamNumber(t => t + 1)}
                >
                    <span className={cn("hidden", !compact && "lg:inline")}>{translate("NextJam")}</span>
                    <ChevronRight />
                </Button>
            </div>
            <MenuColumn 
                teamSide={teamSide}
                skaters={skatingSkaters} 
                skaterPositions={skaterPositions}
                injuredSkaters={injuredSkaters}
                onInjuryAdded={handleInjuryAdded}
                onInjuryRemoved={handleInjuryRemoved} 
            />
            <SkaterNumberColumn 
                teamSide={teamSide}
                skaters={skatingSkaters}
                skaterPositions={skaterPositions} 
                offTrackSkaters={offTrackSkaters} 
                injuredSkaters={injuredSkaters} 
                compact={compact}
                onInjuryAdded={handleInjuryAdded}
                onInjuryRemoved={handleInjuryRemoved} 
            />
            <SkaterPositionColumn
                teamSide={teamSide}
                position={LineupPosition.Bench}
                skaters={skatingSkaters}
                selectedSkaters={skatingSkaters.filter(s => currentJam?.jammerId !== s.id && currentJam?.pivotId !== s.id && !currentJam?.blockerIds.includes(s.id)).map(s => s.id)}
                offTrackSkaters={offTrackSkaters}
                injuredSkaters={injuredSkaters} 
                compact={compact}
                currentJam={totalJamNumber >= jams.length - 1 || totalJamNumber === -1}
                className="col-start-3"
                onSkaterClicked={s => handlePositionClicked(s, LineupPosition.Bench)}
            />
            <SkaterPositionColumn
                teamSide={teamSide}
                position={LineupPosition.Jammer}
                skaters={skatingSkaters}
                selectedSkaters={currentJam?.jammerId ? [currentJam?.jammerId] : []}
                offTrackSkaters={offTrackSkaters}
                injuredSkaters={injuredSkaters} 
                compact={compact}
                currentJam={totalJamNumber >= jams.length - 1 || totalJamNumber === -1}
                className="col-start-4"
                onSkaterClicked={s => handlePositionClicked(s, LineupPosition.Jammer)}
            />
            <SkaterPositionColumn
                teamSide={teamSide}
                position={LineupPosition.Pivot}
                skaters={skatingSkaters}
                selectedSkaters={currentJam?.pivotId ? [currentJam?.pivotId] : []}
                offTrackSkaters={offTrackSkaters}
                injuredSkaters={injuredSkaters} 
                compact={compact}
                currentJam={totalJamNumber >= jams.length - 1 || totalJamNumber === -1}
                className="col-start-5"
                onSkaterClicked={s => handlePositionClicked(s, LineupPosition.Pivot)}
            />
            <SkaterPositionColumn
                teamSide={teamSide}
                position={LineupPosition.Blocker}
                skaters={skatingSkaters}
                selectedSkaters={currentJam?.blockerIds.filter(b => b) as string[] ?? []}
                offTrackSkaters={offTrackSkaters}
                injuredSkaters={injuredSkaters} 
                currentJam={totalJamNumber >= jams.length - 1 || totalJamNumber === -1}
                compact={compact}
                className="col-start-6 border-r-2"
                onSkaterClicked={s => handlePositionClicked(s, LineupPosition.Blocker)}
            />
            <PenaltyBoxColumn
                teamSide={teamSide}
                skaters={skatingSkaters}
                skaterPenalties={skaterPenalties}
                compact={compact}
                onClick={handleBoxClicked}
            />
            <div className="col-start-2 col-span-6 border-t border-black">
            </div>
        </>
    );
}