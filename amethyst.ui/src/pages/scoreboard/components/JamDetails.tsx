import { JamClock, PeriodClock } from "@/components/clocks"
import { ScoreboardComponent } from "./ScoreboardComponent"
import { GameSkater, GameStageState, TeamSide } from "@/types"
import { ClocksBar } from "./ClocksBar";
import { useJamLineupState, useJamStatsState, useTeamDetailsState } from "@/hooks";
import { useMemo } from "react";
import { ScaledText } from "@/components/ScaledText";
import { cn } from "@/lib/utils";

type JamDetailsProps = {
    gameStage: GameStageState;
    visible: boolean;
}

export const JamDetails = ({ gameStage, visible }: JamDetailsProps) => {

    const { jammerNumber: homeJammerNumber, pivotNumber: homePivotNumber } = useJamLineupState(TeamSide.Home) ?? {};
    const { jammerNumber: awayJammerNumber, pivotNumber: awayPivotNumber } = useJamLineupState(TeamSide.Away) ?? {};

    const { team: homeTeam } = useTeamDetailsState(TeamSide.Home) ?? {};
    const { team: awayTeam } = useTeamDetailsState(TeamSide.Away) ?? {};

    const { starPass: homeTeamStarPass} = useJamStatsState(TeamSide.Home) ?? { starPass: false };
    const { starPass: awayTeamStarPass} = useJamStatsState(TeamSide.Away) ?? { starPass: false };

    const homeStats = useJamStatsState(TeamSide.Home);
    const awayStats = useJamStatsState(TeamSide.Away);

    const getJammerText = (jammerNumber: string | undefined, pivotNumber: string | undefined, starPass: boolean, roster?: GameSkater[]) => {
        const skaterNumber = starPass ? pivotNumber : jammerNumber;

        return roster?.find(s => s.number === skaterNumber)?.name ?? skaterNumber ?? "";
    }

    const homeJammerText = useMemo(
        () => getJammerText(homeJammerNumber, homePivotNumber, homeTeamStarPass, homeTeam?.roster),
        [homeTeam, homeJammerNumber, homePivotNumber, homeTeamStarPass]);

    const awayJammerText = useMemo(
        () => getJammerText(awayJammerNumber, awayPivotNumber, awayTeamStarPass, awayTeam?.roster),
        [awayTeam, awayJammerNumber, awayPivotNumber, awayTeamStarPass]);

    const homeIsLead = useMemo(() => homeStats?.lead && !homeStats?.lost, [homeStats]);
    const awayIsLead = useMemo(() => awayStats?.lead && !awayStats?.lost, [awayStats]);

    const jammerNameClassName = "w-full h-full justify-center text-center [-webkit-text-stroke-color:#000] [-webkit-text-stroke-width:.1rem] text-white";

    return (
        <ClocksBar visible={visible} className="flex-col overflow-visible">
            <div className="h-[40%] flex">
                <div className={cn("w-1/2 h-full flex")}>
                    <ScaledText text={homeJammerText} className={cn(jammerNameClassName, homeIsLead ? "animate-pulse-scale" : "")} />
                </div>
                <div className="w-1/2 h-full flex">
                    <ScaledText text={awayJammerText} className={cn(jammerNameClassName, awayIsLead ? "animate-pulse-scale" : "")} />
                </div>
            </div>
            <div className="h-[60%] flex gap-5">
                <ScoreboardComponent className="w-1/2 h-full" header={`Period ${gameStage.periodNumber}`}>
                    <PeriodClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" autoScale />
                </ScoreboardComponent>
                <ScoreboardComponent className="w-1/2 h-full" header={`Jam ${gameStage.jamNumber}`}>
                    <JamClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" autoScale />
                </ScoreboardComponent>
            </div>
        </ClocksBar>
    );
}