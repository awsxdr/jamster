import { useGameStageState } from "@/hooks"
import { Clock } from "./Clock"
import { JamClock, LineupClock, PeriodClock, TimeoutClock } from "@/components/clocks";
import { useI18n } from "@/hooks/I18nHook";
import { IntermissionClock } from "@/components/clocks/IntermissionClock";

export const ClocksContainer = () => {
    const gameStage = useGameStageState();
    const { translate } = useI18n();

    return (
        <div className="w-full flex mt-5 flex-wrap md:flex-nowrap gap-5 justify-between">
            <Clock name={`${translate('ClocksContainer.Jam')} ${gameStage?.jamNumber ?? 0}`} clock={c => <JamClock textClassName={c} />} />
            <Clock name={`${translate('ClocksContainer.Period')} ${gameStage?.periodNumber ?? 0}`} clock={c => <PeriodClock textClassName={c} />} />
            <Clock name={translate('ClocksContainer.Lineup')} clock={c => <LineupClock textClassName={c} />} />
            <Clock name={translate('ClocksContainer.Timeout')} clock={c => <TimeoutClock textClassName={c} />} />
            <Clock name={translate('ClocksContainer.Intermission')} clock={c => <IntermissionClock textClassName={c} />} />
        </div>
    )
}