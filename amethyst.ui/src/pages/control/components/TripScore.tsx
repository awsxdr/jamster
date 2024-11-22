import { Button } from "@/components/ui/button"
import { useI18n } from "@/hooks/I18nHook";

type TripScoreProps = {
    tripScore: number;
}

export const TripScore = ({ tripScore }: TripScoreProps) => {

    const { translate } = useI18n();

    return (
        <div className="flex flex-wrap justify-center items-center m-2 space-x-2">
            <span>{translate("TripScore.TripScore")}</span>
            <span className="flex flex-wrap justify-center items-center m-2 space-x-2 gap-y-2">
                <Button variant={tripScore === 0 ? 'default' : 'secondary'}>0</Button>
                <Button variant={tripScore === 1 ? 'default' : 'secondary'}>1</Button>
                <Button variant={tripScore === 2 ? 'default' : 'secondary'}>2</Button>
                <Button variant={tripScore === 3 ? 'default' : 'secondary'}>3</Button>
                <Button variant={tripScore === 4 ? 'default' : 'secondary'}>4 [🠅S]</Button>
            </span>
        </div>
    )
}