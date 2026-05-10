import { TooltipButton } from "@/components";
import { Card, CardContent, TooltipProvider } from "@/components/ui"
import { useEvents, useGameStageState, useI18n, useOvertimeState, usePostGameClockState, useRulesState, useTeamScoreState } from "@/hooks";
import { cn } from "@/lib/utils"
import { Stage, TeamSide } from "@/types";
import { OvertimeEnded, OvertimeStarted, PeriodFinalized } from "@/types/events";
import { AlarmClockOff, AlarmClockPlus, Lock } from "lucide-react";
import { useMemo } from "react";

type PeriodEndControlPanelProps = {
    gameId?: string;
};

export const PeriodEndControlPanel = ({ gameId }: PeriodEndControlPanelProps) => {
    const { sendEvent } = useEvents();

    const postGameClock = usePostGameClockState();
    const homeTeamScore = useTeamScoreState(TeamSide.Home);
    const awayTeamScore = useTeamScoreState(TeamSide.Away);
    const stage = useGameStageState();
    const overtime = useOvertimeState();
    const rules = useRulesState();

    const shouldBeginOvertime = useMemo(() => stage?.stage === Stage.AfterGame && homeTeamScore?.score == awayTeamScore?.score, [homeTeamScore, awayTeamScore]);
    const shouldFinalizePeriod = useMemo(() => 
        !shouldBeginOvertime && (
            stage?.periodNumber === rules?.rules.periodRules.periodCount && (postGameClock?.secondsPassed ?? 0) > 30
            || (stage?.periodNumber ?? 0) < (rules?.rules.periodRules.periodCount ?? 0)
        ),
    [postGameClock, shouldBeginOvertime]);

    const {translate} = useI18n({ prefix: "ScoreboardControl.PeriodEndControlPanel." });

    const [finalizeText, finalizeDescription, finalizeEnabled] = useMemo(() => {
        if (stage?.periodIsFinalized) {
            return ["---", "", false];
        }

        if(stage?.periodNumber == rules?.rules.periodRules.periodCount) {
            return [translate("FinalizeGame"), translate("FinalizeGame.Description"), !overtime?.isInOvertime];
        } else {
            return [translate("FinalizePeriod"), translate("FinalizePeriod.Description"), true];
        }
    }, [stage, translate]);

    if(!gameId || !stage) {
        return <></>;
    }

    const handleFinalizePeriod = () => {
        sendEvent(gameId, new PeriodFinalized());
    }

    const handleBeginOvertime = () => {
        sendEvent(gameId, new OvertimeStarted());
    }

    const handleEndOvertime = () => {
        sendEvent(gameId, new OvertimeEnded());
    }

    const visible = overtime?.isInOvertime && stage.stage === Stage.Lineup || [Stage.AfterGame, Stage.Intermission].includes(stage.stage) && !stage.periodIsFinalized;

    return (
        <Card className={cn("grow scale-0 transition-all duration-500 h-0 m-0 -mb-2 p-0", visible ? "scale-100 block h-auto mb-0 py-2" : "")}>
            <CardContent className="flex p-0 px-2 flex-wrap gap-2 justify-evenly">
                <TooltipProvider>
                    <TooltipButton
                        id="ScoreboardControl.PeriodEndControlPanel.FinalizePeriod"
                        description={finalizeDescription}
                        className="w-full py-6 md:w-auto md:px-4 md:py-2"
                        variant={(shouldFinalizePeriod && finalizeEnabled ? "default" : "secondary")}
                        disabled={!finalizeEnabled}
                        onClick={handleFinalizePeriod}
                    >
                        <Lock />
                        { finalizeText }
                    </TooltipButton>
                    { !overtime?.isInOvertime && (
                        <TooltipButton
                            id="ScoreboardControl.PeriodEndControlPanel.BeginOvertime"
                            description={translate("BeginOvertime.Description")}
                            className="w-full py-6 md:w-auto md:px-4 md:py-2"
                            variant={(shouldBeginOvertime ? "default" : "secondary")}
                            disabled={stage.stage !== Stage.AfterGame}
                            onClick={handleBeginOvertime}
                        >
                            <AlarmClockPlus />
                            { translate("BeginOvertime") }
                        </TooltipButton>
                    )}
                    { overtime?.isInOvertime && (
                        <TooltipButton
                            id="ScoreboardControl.PeriodEndControlPanel.EndOvertime"
                            description={translate("EndOvertime.Description")}
                            className="w-full py-6 md:w-auto md:px-4 md:py-2"
                            variant="default"
                            onClick={handleEndOvertime}
                        >
                            <AlarmClockOff />
                            { translate("EndOvertime") }
                        </TooltipButton>
                    )}
                </TooltipProvider>
            </CardContent>
        </Card>
    )
}