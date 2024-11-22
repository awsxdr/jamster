import { useTeamScoreState } from "@/hooks";
import { useI18n } from "@/hooks/I18nHook";
import { TeamSide } from "@/types"

type JamScoreProps = {
    side: TeamSide;
}

export const JamScore = ({ side }: JamScoreProps) => {

    const score = useTeamScoreState(side);

    const { translate } = useI18n();

    return (
        <div className="flex w-full justify-center items-center gap-2">
            {translate("JamScore.JamScore")}: <span className="text-5xl">{score?.jamScore}</span>
        </div>
    )
}