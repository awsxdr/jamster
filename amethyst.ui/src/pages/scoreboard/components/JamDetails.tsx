import { JamClock, PeriodClock } from "@/components/clocks"
import { ScoreboardComponent } from "./ScoreboardComponent"
import { GameStageState, TeamSide } from "@/types"
import { ClocksBar } from "./ClocksBar";
import { useJamLineupState, useTeamDetailsState } from "@/hooks";
import { useMemo } from "react";
import { ScaledText } from "@/components/ScaledText";

type JamDetailsProps = {
    gameStage: GameStageState;
    visible: boolean;
}

export const JamDetails = ({ gameStage, visible }: JamDetailsProps) => {

    const { jammerNumber: homeJammerNumber } = useJamLineupState(TeamSide.Home) ?? {};
    const { jammerNumber: awayJammerNumber } = useJamLineupState(TeamSide.Away) ?? {};

    const { team: homeTeam } = useTeamDetailsState(TeamSide.Home) ?? {};
    const { team: awayTeam } = useTeamDetailsState(TeamSide.Away) ?? {};

    const homeJammerText = useMemo(
        () => homeTeam?.roster.find(s => s.number === homeJammerNumber)?.name ?? homeJammerNumber ?? "", 
        [homeTeam, homeJammerNumber]);

    const awayJammerText = useMemo(
        () => awayTeam?.roster.find(s => s.number === awayJammerNumber)?.name ?? awayJammerNumber ?? "", 
        [awayTeam, awayJammerNumber]);
    
    return (
        <div>
            <ClocksBar visible={visible}>
                <div className="w-1/2 h-full flex">
                    <ScaledText text={homeJammerText} className="w-full h-full justify-center text-center text-white" />
                </div>
                <div className="w-1/2 h-full flex">
                    <ScaledText text={awayJammerText} className="w-full h-full justify-center text-center text-white" />
                </div>
            </ClocksBar>
            <ClocksBar visible={visible} className="gap-5">
                <ScoreboardComponent className="w-1/2 h-full" header={`Period ${gameStage.periodNumber}`}>
                    <PeriodClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" autoScale />
                </ScoreboardComponent>
                <ScoreboardComponent className="w-1/2 h-full" header={`Jam ${gameStage.jamNumber}`}>
                    <JamClock textClassName="flex justify-center items-center h-full m-2 overflow-hidden" autoScale />
                </ScoreboardComponent>
            </ClocksBar>
        </div>
    );
}