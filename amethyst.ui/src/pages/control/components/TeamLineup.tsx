import { RadioButtonGroup } from "@/components/RadioButtonGroup";
import { useI18n, useJamLineupState, useTeamDetailsState } from "@/hooks";
import { TeamSide } from "@/types";
import { SkaterPosition } from "@/types/events/JamLineup";

type TeamLineupProps = {
    side: TeamSide;
    onLineupSelected?: (position: SkaterPosition, number: string | null) => void;
    disabled?: boolean;
}

export const TeamLineup = ({ side, onLineupSelected, disabled }: TeamLineupProps) => {
    const lineup = useJamLineupState(side);
    const team = useTeamDetailsState(side);
    
    const skaterNumbers = team?.team.roster.filter(s => s.isSkating).map(s => s.number).sort() ?? [];

    const { translate } = useI18n();

    return (
        <>
            <div className="flex justify-center items-center self-center">
                <div className="flex flex-col items-end">
                    <div className="flex flex-wrap justify-center items-center gap-2 pt-5 pb-1 items-baseline">
                        <span className={disabled ? "opacity-50" : ""}>{translate("TeamLineup.Jammer")}</span>
                        <RadioButtonGroup
                            items={[{value: null, name: "?"}, ...skaterNumbers.map(s => ({ value: s, name: s}))]}
                            value={lineup?.jammerNumber}
                            rowClassName="gap-0.5"
                            variant="ghost"
                            size="sm"
                            disabled={disabled}
                            onItemSelected={v => onLineupSelected?.(SkaterPosition.Jammer, v)}
                        />
                    </div>
                    <div className="flex flex-wrap justify-center items-center gap-2 pb-5 items-baseline">
                        <span className={disabled ? "opacity-50" : ""}>{translate("TeamLineup.Pivot")}</span>
                        <RadioButtonGroup
                            items={[{value: null, name: "?"}, ...skaterNumbers.map(s => ({ value: s, name: s}))]}
                            value={lineup?.pivotNumber}
                            rowClassName="gap-0.5"
                            size="sm"
                            variant="ghost"
                            disabled={disabled}
                            onItemSelected={v => onLineupSelected?.(SkaterPosition.Pivot, v)}
                        />
                    </div>
                </div>
            </div>
        </>
    );
}