import { RadioButtonGroup } from "@/components/RadioButtonGroup";
import { useI18n } from "@/hooks/I18nHook";

type TripScoreProps = {
    tripScore: number;
    scoreShortcutKeys?: string[];
    disabled?: boolean;
    onTripScoreSet?: (tripScore: number) => void;
}

export const TripScore = ({ tripScore, scoreShortcutKeys, disabled, onTripScoreSet }: TripScoreProps) => {

    const { translate } = useI18n();

    return (
        <div className="flex flex-wrap justify-center items-center m-2 space-x-2">
            <span className={disabled ? "opacity-50" : ""}>{translate("TripScore.TripScore")}</span>
            <RadioButtonGroup
                items={[0, 1, 2, 3, 4].map(i => ({ value: i, name: `${i}${ scoreShortcutKeys?.[i] ? ` [${scoreShortcutKeys[i]}]` : ''}`}))}
                value={tripScore}
                disabled={disabled}
                onItemSelected={onTripScoreSet}
            />
        </div>
    )
}