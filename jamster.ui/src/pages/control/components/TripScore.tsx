import { RadioButtonGroup } from "@/components/RadioButtonGroup";
import { useI18n } from "@/hooks/I18nHook";
import { TeamSide } from "@/types";

type TripScoreProps = {
    teamSide: TeamSide;
    tripScore: number;
    disabled?: boolean;
    onTripScoreSet?: (tripScore: number) => void;
}

export const TripScore = ({ teamSide, tripScore, disabled, onTripScoreSet }: TripScoreProps) => {

    const { translate } = useI18n({ prefix: "ScoreboardControl.TripScore." });

    return (
        <div className="flex flex-wrap justify-center items-center m-2 space-x-2">
            <span className={disabled ? "opacity-50" : ""}>{translate("TripScore")}</span>
            <RadioButtonGroup
                items={['-', 0, 1, 2, 3, 4].map((k, i) => ({ 
                    id: `ScoreboardControl.TripScore.${teamSide}.${k}`,
                    value: i - 1, 
                    name: k.toString(), 
                    description: translate(`SetScore${k}`), 
                    shortcutGroup: `${teamSide.toLowerCase()}Score`,
                    shortcutKey: `setTripScore${k === '-' ? "Unknown" : k}`
                }))}
                rowClassName="flex-wrap sm:flex-nowrap"
                value={tripScore}
                disabled={disabled}
                onItemSelected={onTripScoreSet}
            />
        </div>
    )
}