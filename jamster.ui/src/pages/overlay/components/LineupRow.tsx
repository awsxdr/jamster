import { useGameStageState, useI18n, useJamLineupState, useJamStatsState, useTeamDetailsState } from "@/hooks";
import { cn } from "@/lib/utils";
import { Stage, TeamSide } from "@/types";

type LineupRowProps = {
    side: TeamSide;
}

export const LineupRow = ({ side }: LineupRowProps) => {

    const { stage } = useGameStageState() ?? { };
    const lineup = useJamLineupState(side);
    const { team } = useTeamDetailsState(side) ?? { };
    const { starPass } = useJamStatsState(side) ?? { };

    const { translate } = useI18n();

    const visible = stage === Stage.Jam;

    const sharedRowClassName = "absolute flex items-center h-[--lineup-row-height] w-0 left-[--lineup-row-left] [font-size:var(--lineup-row-text-size)] leading-[--lineup-row-text-size] overflow-hidden text-nowrap bg-[rgb(0,0,0,0.8)] transition-[width] duration-500";
    const homeRowClassName = cn(sharedRowClassName, "top-[--lineup-row-top] rounded-t-lg");
    const awayRowClassName = cn(sharedRowClassName, "top-[calc(var(--lineup-row-top)_+_var(--lineup-row-height))] rounded-b-lg");

    const sharedRowItemClassName = cn(
        "flex items-center h-full text-white opacity-0 transition-opacity duration-200",
        visible && "opacity-100",
        visible && "delay-300",
    );
    const jammerNameClassName = cn(sharedRowItemClassName, "justify-end w-[--lineup-jammer-name-width] pr-[2%]");
    const skaterNumberClassName = cn(sharedRowItemClassName, "justify-center w-[--lineup-skater-width] border-[#ddd] border-l-[1px]");

    const [jammerId, pivotId] = starPass ? [lineup?.pivotId, lineup?.jammerId] : [lineup?.jammerId, lineup?.pivotId];
    const jammer = team?.roster.find(s => s.id === jammerId) ?? { number: '-', name: undefined };
    const pivot = team?.roster.find(s => s.id === pivotId) ?? { number: '-' };

    const jammerText =
        jammer.name
            ? `${jammer.name} (${jammer.number})`
            : jammer.number;

    const blockers = [...(lineup?.blockerIds ?? [])].map(id => team?.roster.find(s => s.id === id) ?? { number: '-' }).sort();

    return (
        <div className={cn(side === TeamSide.Home ? homeRowClassName : awayRowClassName, visible && 'w-[--lineup-row-width]')}>
            <div className={jammerNameClassName}>{jammerText}</div>
            <div className={skaterNumberClassName}>{pivot.number ?? '-'}{starPass && translate("Overlay.LineupRow.StarPassMarker")}</div>
            <div className={skaterNumberClassName}>{blockers[0]?.number ?? '-'}</div>
            <div className={skaterNumberClassName}>{blockers[1]?.number ?? '-'}</div>
            <div className={skaterNumberClassName}>{blockers[2]?.number ?? '-'}</div>
        </div>
    )
}