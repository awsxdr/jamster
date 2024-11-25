import { Button } from "@/components/ui/button"
import { useI18n } from "@/hooks/I18nHook";

type TripScoreProps = {
    tripScore: number;
    scoreShortcutKeys?: string[];
    onTripScoreSet?: (tripScore: number) => void;
}

export const TripScore = ({ tripScore, scoreShortcutKeys, onTripScoreSet }: TripScoreProps) => {

    const { translate } = useI18n();

    return (
        <div className="flex flex-wrap justify-center items-center m-2 space-x-2">
            <span>{translate("TripScore.TripScore")}</span>
            <span className="flex flex-wrap justify-center items-center m-2 space-x-2 gap-y-2">
                <Button 
                    variant={tripScore === 0 ? 'default' : 'secondary'} 
                    onClick={() => onTripScoreSet?.(0)}
                >
                    0{ scoreShortcutKeys?.[0] ? ` [${scoreShortcutKeys[0]}]` : ''}
                </Button>
                <Button 
                    variant={tripScore === 1 ? 'default' : 'secondary'} 
                    onClick={() => onTripScoreSet?.(1)}
                >
                    1{ scoreShortcutKeys?.[1] ? ` [${scoreShortcutKeys[1]}]` : ''}
                </Button>
                <Button 
                    variant={tripScore === 2 ? 'default' : 'secondary'} 
                    onClick={() => onTripScoreSet?.(2)}
                >
                    2{ scoreShortcutKeys?.[2] ? ` [${scoreShortcutKeys[2]}]` : ''}
                </Button>
                <Button 
                    variant={tripScore === 3 ? 'default' : 'secondary'} 
                    onClick={() => onTripScoreSet?.(3)}
                >
                    3{ scoreShortcutKeys?.[3] ? ` [${scoreShortcutKeys[3]}]` : ''}
                </Button>
                <Button 
                    variant={tripScore === 4 ? 'default' : 'secondary'} 
                    onClick={() => onTripScoreSet?.(4)}
                >
                    4{ scoreShortcutKeys?.[4] ? ` [${scoreShortcutKeys[4]}]` : ''}
                </Button>
            </span>
        </div>
    )
}