import { RadioButtonGroup } from "@/components/RadioButtonGroup";
import { useI18n, useJamLineupState, useTeamDetailsState } from "@/hooks";
import { TeamSide } from "@/types";
import { SkaterPosition } from "@/types/events/Lineup";

type TeamLineupProps = {
    side: TeamSide;
    onLineupSelected?: (position: SkaterPosition, number: string | null, currentNumber: string | undefined) => void;
    disabled?: boolean;
}

export const TeamLineup = ({ side, onLineupSelected, disabled }: TeamLineupProps) => {
    const lineup = useJamLineupState(side);
    const { team } = useTeamDetailsState(side) ?? {};
    
    const skatingSkaters = team?.roster.filter(s => s.isSkating).sort((a, b) => a.number < b.number ? -1 : a.number > b.number ? 1 : 0) ?? [];

    const { translate } = useI18n();

    return (
        <>
            <div className="flex justify-center items-center self-center">
                <div className="flex flex-col items-end">
                    <div className="flex flex-wrap justify-center items-center gap-2 p-2 pb-1 items-baseline">
                        <span className={disabled ? "opacity-50" : ""}>{translate("TeamLineup.Jammer")}</span>
                        <RadioButtonGroup
                            items={[
                                {value: null, name: "?", id: `ScoreboardControl.TeamLineup.${side}.Jammer.Unknown`},
                                ...skatingSkaters.map(s => ({ value: s.id, name: s.number, id: `ScoreboardControl.TeamLineup.${side}.Jammer.${s.id}`}))
                            ]}
                            value={lineup?.jammerId}
                            rowClassName="gap-0.5"
                            variant="ghost"
                            size="sm"
                            disabled={disabled}
                            onItemSelected={v => onLineupSelected?.(SkaterPosition.Jammer, v, lineup?.jammerId)}
                        />
                    </div>
                    <div className="flex flex-wrap justify-center items-center gap-2 p-2 pt-0 items-baseline">
                        <span className={disabled ? "opacity-50" : ""}>{translate("TeamLineup.Pivot")}</span>
                        <RadioButtonGroup
                            items={[
                                {value: null, name: "?", id: `ScoreboardControl.TeamLineup.${side}.Pivot.Unknown`},
                                ...skatingSkaters.map(s => ({ value: s.id, name: s.number, id: `ScoreboardControl.TeamLineup.${side}.Pivot.${s.id}`}))
                            ]}
                            value={lineup?.pivotId}
                            rowClassName="gap-0.5"
                            size="sm"
                            variant="ghost"
                            disabled={disabled}
                            onItemSelected={v => onLineupSelected?.(SkaterPosition.Pivot, v, lineup?.pivotId)}
                        />
                    </div>
                </div>
            </div>
        </>
    );
}