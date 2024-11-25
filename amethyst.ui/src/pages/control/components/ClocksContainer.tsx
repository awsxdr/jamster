import { useGameStageState } from "@/hooks"
import { Clock } from "./Clock"
import { JamClock, LineupClock, PeriodClock, TimeoutClock } from "@/components/clocks";
import { useI18n } from "@/hooks/I18nHook";
import { IntermissionClock } from "@/components/clocks/IntermissionClock";

export const ClocksContainer = () => {
    const gameStage = useGameStageState();
    const { translate } = useI18n();

    return (
        <div className="w-full flex space-x-5 mt-5">
            <Clock name={`${translate('Jam')} ${gameStage?.jamNumber ?? 0}`} clock={c => <JamClock textClassName={c} />} />
            <Clock name={`${translate('Period')} ${gameStage?.periodNumber ?? 0}`} clock={c => <PeriodClock textClassName={c} />} />
            <Clock name={translate('Lineup')} clock={c => <LineupClock textClassName={c} />} />
            <Clock name={translate('Timeout')} clock={c => <TimeoutClock textClassName={c} />} />
            <Clock name={translate('Intermission')} clock={c => <IntermissionClock textClassName={c} />} />
        </div>
    )
}