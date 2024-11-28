import { Button } from "@/components/ui/button"
import { useI18n } from "@/hooks/I18nHook";
import { cn } from "@/lib/utils";

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
            <span>{translate("TripScore.TripScore")}</span>
            <span className="flex flex-wrap justify-center items-center m-2 space-x-2 gap-y-2">
                <Button 
                    variant="secondary" 
                    disabled={disabled}
                    className={cn("border-4", tripScore === 0 ? "border-lime-600" : "") }
                    onClick={() => onTripScoreSet?.(0)}
                >
                    0{ scoreShortcutKeys?.[0] ? ` [${scoreShortcutKeys[0]}]` : ''}
                </Button>
                <Button 
                    variant="secondary" 
                    disabled={disabled}
                    className={cn("border-4", tripScore === 1 ? "border-lime-600" : "") }
                    onClick={() => onTripScoreSet?.(1)}
                >
                    1{ scoreShortcutKeys?.[1] ? ` [${scoreShortcutKeys[1]}]` : ''}
                </Button>
                <Button 
                    variant="secondary" 
                    disabled={disabled}
                    className={cn("border-4", tripScore === 2 ? "border-lime-600" : "") }
                    onClick={() => onTripScoreSet?.(2)}
                >
                    2{ scoreShortcutKeys?.[2] ? ` [${scoreShortcutKeys[2]}]` : ''}
                </Button>
                <Button 
                    variant="secondary" 
                    disabled={disabled}
                    className={cn("border-4", tripScore === 3 ? "border-lime-600" : "") }
                    onClick={() => onTripScoreSet?.(3)}
                >
                    3{ scoreShortcutKeys?.[3] ? ` [${scoreShortcutKeys[3]}]` : ''}
                </Button>
                <Button 
                    variant="secondary" 
                    disabled={disabled}
                    className={cn("border-4", tripScore === 4 ? "border-lime-600" : "") }
                    onClick={() => onTripScoreSet?.(4)}
                >
                    4{ scoreShortcutKeys?.[4] ? ` [${scoreShortcutKeys[4]}]` : ''}
                </Button>
            </span>
        </div>
    )
}